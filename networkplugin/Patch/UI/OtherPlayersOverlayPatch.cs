using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// "其他玩家角色实体更新/渲染"补丁（参考 Together in Spire：CharacterEntityUpdateAndRender.java）
/// <para/>
/// LBoL 是 Unity 渲染管线，通常不直接 Patch "render()"，因此本补丁采用：
/// 1) Patch `GameDirector.Update()` 作为全局每帧入口（类比 AbstractDungeon.update / AbstractRoom.update）
/// 2) 通过 Unity UI 在屏幕上生成"玩家信息框 + 翻页按钮"（类比 RenderInfoBoxes/UpdateInfoBoxes）
/// 3) 通过监听网络事件维护"其他玩家列表"（类比 P2PManager.GetAllPlayers）
/// </summary>
[HarmonyPatch]
public static partial class OtherPlayersOverlayPatch
{
    #region 常量和字段

    private const float AvatarEntryBaseWidth = 140f;
    private const float AvatarEntryBaseHeight = 176f;
    private const float AvatarVisualSize = 112f;
    private const float AvatarEntrySpacing = 22f;
    private const float AvatarStripYOffset = 14f;

    /// <summary>获取依赖注入容器</summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>用于同步访问玩家列表的锁</summary>
    private static readonly object _syncLock = new();

    /// <summary>存储所有玩家信息的字典，key为PlayerId</summary>
    private static readonly Dictionary<string, PlayerSummary> _players = new();

    /// <summary>overlay UI实例</summary>
    private static OverlayUi _ui;

    /// <summary>默认字体资源（缓存）</summary>
    private static TMP_FontAsset _defaultFont;

    /// <summary>当前订阅的网络客户端</summary>
    private static INetworkClient _subscribedClient;

    /// <summary>是否已订阅网络事件</summary>
    private static bool _subscribed;

    /// <summary>本地客户端在服务器侧的 PlayerId（用于过滤自身渲染）</summary>
    private static string _selfPlayerId;

    /// <summary>战斗场景中远程玩家角色根节点</summary>
    private static Transform _remoteCharactersRoot;

    /// <summary>战斗场景中远程玩家角色视图缓存（PlayerId -> View）</summary>
    private static readonly Dictionary<string, RemoteCharacterView> _remoteCharacters = new();

    /// <summary>地图面板中远程玩家图标根节点</summary>
    private static RectTransform _mapIconsRoot;

    /// <summary>地图面板中远程玩家图标缓存（PlayerId -> Icon）</summary>
    private static readonly Dictionary<string, MapIconUi> _mapIcons = new();

    /// <summary>角色头像缓存（CharacterId -> Sprite）</summary>
    private static readonly Dictionary<string, Sprite> _avatarCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>游戏事件接收委托（用于事件订阅）</summary>
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;

    /// <summary>连接状态变化委托（用于事件订阅）</summary>
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    #endregion

    #region Harmony 补丁方法

    /// <summary>
    /// GameDirector.Update() 的后处理补丁
    /// 每帧执行，负责更新网络连接状态、处理事件、以及刷新UI
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), "Update")]
    [HarmonyPostfix]
    public static void GameDirector_Update_Postfix()
    {
        try
        {
            // 从依赖注入容器获取当前网络客户端实例
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
            {
                // 没有网络客户端时：若启用虚拟玩家，则仍允许远程渲染；否则按原逻辑隐藏。
                EnsureVirtualAiDefaultPlayer_NoThrow();
                if (!IsVirtualAiDefaultEnabled())
                {
                    HideUi();
                    HideRemoteCharacters();
                    return;
                }

                HideUi();
                EnsureRemoteCharacters();
                UpdateRemoteCharactersLayout();
                return;
            }

            // 确保已经为当前客户端完成事件订阅（连接状态/游戏事件）
            EnsureSubscribed(client);

            // 每帧轮询一次网络事件，让玩家列表相关事件尽快被处理
            try
            {
                client.PollEvents();
            }
            catch
            {
                // 忽略轮询失败：某些实现要求在 Start/Connect 之后才能 PollEvents
            }

            // 如果当前网络未连接，则不显示 Overlay
            if (!client.IsConnected)
            {
                EnsureVirtualAiDefaultPlayer_NoThrow();
                if (!IsVirtualAiDefaultEnabled())
                {
                    HideUi();
                    HideRemoteCharacters();
                    return;
                }

                HideUi();
                EnsureRemoteCharacters();
                UpdateRemoteCharactersLayout();
                return;
            }

            // 在加载界面或输入被阻塞时不显示 Overlay，避免挡住游戏 UI
            // UiManager 的大部分状态是私有字段，这里优先使用公开属性
            if (UiManager.Instance == null || UiManager.IsShowingLoading || UiManager.IsBlockingInput)
            {
                HideUi();
                HideRemoteCharacters();
                return;
            }

            if (IsBattleOverlayActive())
            {
                EnsureUi();
                RefreshUi();
            }
            else
            {
                HideUi();
            }

            // 渲染远程玩家“角色实体”（战斗场景）
            EnsureVirtualAiDefaultPlayer_NoThrow();
            EnsureRemoteCharacters();
            UpdateRemoteCharactersLayout();
        }
        catch (Exception ex)
        {
            // 捕获补丁逻辑中的所有异常，防止影响游戏主循环
            Plugin.Logger?.LogError($"[OtherPlayersOverlayPatch] GameDirector_Update_Postfix 异常: {ex}");
        }
    }


    /// <summary>
    /// GameDirector.MasterTick() 的后处理补丁
    /// 用于驱动远程玩家 UnitView 的 Tick（否则不会被 GameDirector 维护的列表更新）
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), "MasterTick")]
    [HarmonyPostfix]
    private static void GameDirector_MasterTick_Postfix()
    {
        try
        {
            TickRemoteCharacters();
        }
        catch
        {
            // 忽略：避免影响主循环
        }
    }

    /// <summary>
    /// MapPanel.UpdateMapNodesStatus() 的后处理补丁
    /// 在地图面板刷新节点状态时，附带刷新远程玩家在地图上的头像标记
    /// </summary>
    [HarmonyPatch(typeof(MapPanel), "UpdateMapNodesStatus")]
    [HarmonyPostfix]
    public static void MapPanel_UpdateMapNodesStatus_Postfix(MapPanel __instance)
    {
        try
        {
            UpdateMapIcons(__instance);
        }
        catch
        {
            // 忽略：避免影响 UI
        }
    }

    #endregion

    #region UI 创建和更新

    private static void EnsureUi()
    {
        if (_ui != null && _ui.Root != null)
        {
            return;
        }

        Transform parent = TryGetUiLayerTransform("topLayer") ?? TryGetUiLayerTransform("topmostLayer") ?? UiManager.Instance?.transform;
        if (parent == null)
        {
            return;
        }

        _defaultFont ??= FindDefaultFont(parent);

        GameObject root = new("NetworkPlugin_OtherPlayersOverlay");
        root.transform.SetParent(parent, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.anchoredPosition = new Vector2(0f, 0f);
        rootRect.sizeDelta = new Vector2(900f, AvatarEntryBaseHeight + 34f);

        GameObject entriesRootGo = new("EntriesRoot");
        entriesRootGo.transform.SetParent(root.transform, false);
        RectTransform entriesRect = entriesRootGo.AddComponent<RectTransform>();
        entriesRect.anchorMin = new Vector2(0.5f, 1f);
        entriesRect.anchorMax = new Vector2(0.5f, 1f);
        entriesRect.pivot = new Vector2(0.5f, 1f);
        entriesRect.anchoredPosition = Vector2.zero;
        entriesRect.sizeDelta = new Vector2(900f, AvatarEntryBaseHeight);

        _ui = new OverlayUi
        {
            Root = root,
            RootRect = rootRect,
            EntriesRoot = entriesRect,
            Entries = new Dictionary<string, AvatarEntryUi>(StringComparer.Ordinal)
        };

        root.SetActive(false);
    }

    private static void RefreshUi()
    {
        List<PlayerSummary> list;
        lock (_syncLock)
        {
            list = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || !string.Equals(p.PlayerId, _selfPlayerId, StringComparison.Ordinal))
                .OrderByDescending(p => p.IsHost)
                .ThenByDescending(p => p.IsConnected)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (list.Count == 0)
        {
            HideUi();
            return;
        }

        EnsureUi();
        if (_ui?.Root == null)
        {
            return;
        }

        if (!TryAttachUiToBaseMana())
        {
            HideUi();
            return;
        }

        EnsureAvatarTemplate();
        if (_ui.AvatarTemplate == null)
        {
            HideUi();
            return;
        }

        _ui.Root.SetActive(true);

        HashSet<string> alive = new(list.Select(p => p.PlayerId), StringComparer.Ordinal);
        foreach (var kv in _ui.Entries.ToList())
        {
            if (alive.Contains(kv.Key))
            {
                continue;
            }

            if (kv.Value?.Root != null)
            {
                UnityEngine.Object.Destroy(kv.Value.Root);
            }

            _ui.Entries.Remove(kv.Key);
        }

        var orderedEntries = new List<AvatarEntryUi>(list.Count);
        foreach (PlayerSummary player in list)
        {
            AvatarEntryUi entry = EnsureAvatarEntry(player.PlayerId);
            if (entry == null)
            {
                continue;
            }

            ApplyAvatarEntry(entry, player);
            orderedEntries.Add(entry);
        }

        if (orderedEntries.Count == 0)
        {
            HideUi();
            return;
        }

        LayoutAvatarEntries(orderedEntries);
    }

    private static void HideUi()
    {
        if (_ui?.Root != null)
        {
            _ui.Root.SetActive(false);
        }
    }

    #endregion

    private static void EnsureAvatarTemplate()
    {
        if (_ui == null || _ui.EntriesRoot == null || _ui.AvatarTemplate != null)
        {
            return;
        }

        GameObject source = null;
        try
        {
            UltimateSkillPanel panel = UiManager.GetPanel<UltimateSkillPanel>();
            if (panel != null)
            {
                source = panel.gameObject;
            }
        }
        catch
        {
            // ignored
        }

        if (source == null)
        {
            try
            {
                source = Resources.Load<GameObject>("UI/Panels/UltimateSkillPanel");
            }
            catch
            {
                // ignored
            }
        }

        if (source == null)
        {
            return;
        }

        GameObject template = UnityEngine.Object.Instantiate(source, _ui.EntriesRoot, false);
        template.name = "RemotePlayerAvatarTemplate";
        PrepareAvatarTemplate(template);
        template.SetActive(false);
        _ui.AvatarTemplate = template;
    }

    private static void PrepareAvatarTemplate(GameObject template)
    {
        if (template == null)
        {
            return;
        }

        foreach (Graphic graphic in template.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic != null)
            {
                graphic.raycastTarget = false;
            }
        }

        foreach (ParticleSystem particle in template.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (particle == null)
            {
                continue;
            }

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.gameObject.SetActive(false);
        }

        UltimateSkillPanel panel = template.GetComponent<UltimateSkillPanel>();
        if (panel != null)
        {
            panel.enabled = false;
            HideUltimateVisualField(panel, "powerText");
            HideUltimateVisualField(panel, "gauge1");
            HideUltimateVisualField(panel, "gauge2");
            HideUltimateVisualField(panel, "gauge3");
            HideUltimateVisualField(panel, "fireParticle1");
            HideUltimateVisualField(panel, "fireParticle2");
            HideUltimateVisualField(panel, "fireParticle3");
            HideUltimateVisualField(panel, "lightParticle");
        }

        CanvasGroup group = template.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 1f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    private static void HideUltimateVisualField(UltimateSkillPanel panel, string fieldName)
    {
        if (panel == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return;
        }

        try
        {
            Component component = Traverse.Create(panel).Field(fieldName).GetValue<Component>();
            if (component != null)
            {
                component.gameObject.SetActive(false);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static AvatarEntryUi EnsureAvatarEntry(string playerId)
    {
        if (_ui == null || _ui.EntriesRoot == null || _ui.AvatarTemplate == null || string.IsNullOrWhiteSpace(playerId))
        {
            return null;
        }

        if (_ui.Entries.TryGetValue(playerId, out AvatarEntryUi existing) && existing?.Root != null)
        {
            return existing;
        }

        GameObject root = new($"AvatarEntry_{playerId}");
        root.transform.SetParent(_ui.EntriesRoot, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(AvatarEntryBaseWidth, AvatarEntryBaseHeight);

        GameObject visual = UnityEngine.Object.Instantiate(_ui.AvatarTemplate, root.transform, false);
        visual.name = "Visual";
        visual.SetActive(true);

        RectTransform visualRect = visual.GetComponent<RectTransform>();
        if (visualRect == null)
        {
            visualRect = visual.AddComponent<RectTransform>();
        }

        visualRect.anchorMin = new Vector2(0.5f, 1f);
        visualRect.anchorMax = new Vector2(0.5f, 1f);
        visualRect.pivot = new Vector2(0.5f, 1f);
        visualRect.anchoredPosition = Vector2.zero;
        visualRect.sizeDelta = new Vector2(AvatarVisualSize, AvatarVisualSize);

        Image avatar = TryGetAvatarImageFromTemplate(visual) ?? CreateFallbackAvatarVisual(visual.transform);
        avatar.raycastTarget = false;
        avatar.preserveAspect = true;

        TextMeshProUGUI name = CreateTmpText(root.transform, "Name", playerId, 15f);
        name.alignment = TextAlignmentOptions.Center;
        RectTransform nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 0f);
        nameRect.pivot = new Vector2(0.5f, 0f);
        nameRect.anchoredPosition = new Vector2(0f, 20f);
        nameRect.sizeDelta = new Vector2(0f, 20f);

        TextMeshProUGUI status = CreateTmpText(root.transform, "Status", "在线", 13f);
        status.alignment = TextAlignmentOptions.Center;
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(0.5f, 0f);
        statusRect.anchoredPosition = new Vector2(0f, 2f);
        statusRect.sizeDelta = new Vector2(0f, 18f);

        var entry = new AvatarEntryUi
        {
            PlayerId = playerId,
            Root = root,
            RootRect = rootRect,
            Avatar = avatar,
            Name = name,
            Status = status,
        };

        _ui.Entries[playerId] = entry;
        return entry;
    }

    private static Image TryGetAvatarImageFromTemplate(GameObject visual)
    {
        if (visual == null)
        {
            return null;
        }

        try
        {
            UltimateSkillPanel panel = visual.GetComponent<UltimateSkillPanel>();
            if (panel != null)
            {
                Image skillImage = Traverse.Create(panel).Field("skillImage").GetValue<Image>();
                if (skillImage != null)
                {
                    return skillImage;
                }
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            return visual.GetComponentsInChildren<Image>(true).FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static Image CreateFallbackAvatarVisual(Transform parent)
    {
        GameObject go = new("Avatar");
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(AvatarVisualSize * 0.85f, AvatarVisualSize * 0.85f);

        Image image = go.AddComponent<Image>();
        image.sprite = GetWhiteSprite();
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = true;
        return image;
    }

    private static void ApplyAvatarEntry(AvatarEntryUi entry, PlayerSummary player)
    {
        if (entry == null || player == null || entry.Root == null)
        {
            return;
        }

        entry.Root.SetActive(true);

        bool isConnected = player.IsConnected;
        string hostTag = player.IsHost ? " [房主]" : string.Empty;
        string displayName = ResolveDisplayName(player.PlayerId, player.PlayerName);

        if (entry.Name != null)
        {
            entry.Name.text = $"{displayName}{hostTag}";
            entry.Name.color = isConnected ? Color.white : new Color(0.78f, 0.78f, 0.78f, 1f);
        }

        if (entry.Status != null)
        {
            entry.Status.text = isConnected ? "在线" : "离线";
            entry.Status.color = isConnected ? new Color(0.72f, 1f, 0.72f, 1f) : new Color(1f, 0.66f, 0.66f, 1f);
        }

        if (entry.Avatar != null)
        {
            entry.Avatar.sprite = TryGetAvatarSprite(player.CharacterId) ?? GetWhiteSprite();
            entry.Avatar.color = isConnected ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.95f);
        }
    }

    private static void LayoutAvatarEntries(List<AvatarEntryUi> entries)
    {
        if (_ui?.EntriesRoot == null || entries == null || entries.Count == 0)
        {
            return;
        }

        float availableWidth = _ui.EntriesRoot.rect.width;
        if (availableWidth <= 0f)
        {
            availableWidth = 900f;
        }

        float contentWidth = Mathf.Max(360f, availableWidth - 24f);
        float requiredWidth = entries.Count * AvatarEntryBaseWidth + Mathf.Max(0, entries.Count - 1) * AvatarEntrySpacing;
        float scale = requiredWidth > contentWidth
            ? Mathf.Clamp(contentWidth / requiredWidth, 0.45f, 1f)
            : 1f;

        float itemWidth = AvatarEntryBaseWidth * scale;
        float spacing = AvatarEntrySpacing * scale;
        float totalWidth = entries.Count * itemWidth + Mathf.Max(0, entries.Count - 1) * spacing;
        float startX = -totalWidth * 0.5f + itemWidth * 0.5f;

        for (int i = 0; i < entries.Count; i++)
        {
            AvatarEntryUi entry = entries[i];
            if (entry?.RootRect == null)
            {
                continue;
            }

            entry.RootRect.anchorMin = new Vector2(0.5f, 1f);
            entry.RootRect.anchorMax = new Vector2(0.5f, 1f);
            entry.RootRect.pivot = new Vector2(0.5f, 1f);
            entry.RootRect.sizeDelta = new Vector2(AvatarEntryBaseWidth, AvatarEntryBaseHeight);
            entry.RootRect.localScale = new Vector3(scale, scale, 1f);
            entry.RootRect.anchoredPosition = new Vector2(startX + i * (itemWidth + spacing), 0f);
        }
    }

    /// <summary>
    /// 创建TextMeshPro文本控件
    /// </summary>
    /// <param name="parent">父容器</param>
    /// <param name="name">文本对象名称</param>
    /// <param name="text">初始文本内容</param>
    /// <param name="fontSize">字体大小</param>
    /// <returns>创建的TextMeshProUGUI组件</returns>
    private static TextMeshProUGUI CreateTmpText(Transform parent, string name, string text, float fontSize)
    {
        // 创建文本容器
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        // 创建并配置TextMeshProUGUI组件
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.raycastTarget = false;  // 不阻挡射线检测

        // 应用默认字体（如果已缓存）
        if (_defaultFont != null)
        {
            tmp.font = _defaultFont;
        }
        return tmp;
    }

    #region 工具方法

    private static bool IsBattleOverlayActive()
    {
        try
        {
            GameDirector director = Singleton<GameDirector>.Instance;
            return director != null && director.PlayerUnitView != null;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryAttachUiToBaseMana()
    {
        if (_ui?.RootRect == null)
        {
            return false;
        }

        RectTransform anchor = TryGetBaseManaAnchorRect();
        RectTransform parentRect = anchor != null ? anchor.parent as RectTransform : null;

        if (parentRect == null)
        {
            Transform fallback = TryGetUiLayerTransform("topLayer") ?? TryGetUiLayerTransform("topmostLayer") ?? UiManager.Instance?.transform;
            parentRect = fallback as RectTransform;
            if (parentRect == null)
            {
                return false;
            }

            if (_ui.RootRect.parent != parentRect)
            {
                _ui.RootRect.SetParent(parentRect, false);
            }

            _ui.RootRect.anchorMin = new Vector2(0.5f, 1f);
            _ui.RootRect.anchorMax = new Vector2(0.5f, 1f);
            _ui.RootRect.pivot = new Vector2(0.5f, 1f);
            _ui.RootRect.anchoredPosition = new Vector2(0f, -180f);
        }
        else
        {
            if (_ui.RootRect.parent != parentRect)
            {
                _ui.RootRect.SetParent(parentRect, false);
            }

            Vector3 worldBottomCenter = anchor.TransformPoint(new Vector3(anchor.rect.center.x, anchor.rect.yMin, 0f));
            Vector3 localPoint = parentRect.InverseTransformPoint(worldBottomCenter);

            _ui.RootRect.anchorMin = new Vector2(0.5f, 0.5f);
            _ui.RootRect.anchorMax = new Vector2(0.5f, 0.5f);
            _ui.RootRect.pivot = new Vector2(0.5f, 1f);
            _ui.RootRect.anchoredPosition = new Vector2(localPoint.x, localPoint.y - AvatarStripYOffset);
        }

        float parentWidth = parentRect.rect.width;
        float width = parentWidth > 0f ? Mathf.Clamp(parentWidth * 0.6f, 520f, 1500f) : 900f;
        _ui.RootRect.sizeDelta = new Vector2(width, AvatarEntryBaseHeight + 34f);

        if (_ui.EntriesRoot != null)
        {
            _ui.EntriesRoot.sizeDelta = new Vector2(width, AvatarEntryBaseHeight);
            _ui.EntriesRoot.anchoredPosition = Vector2.zero;
        }

        return true;
    }

    private static RectTransform TryGetBaseManaAnchorRect()
    {
        try
        {
            SystemBoard board = UiManager.GetPanel<SystemBoard>();
            if (board == null)
            {
                return null;
            }

            RectTransform baseManaContent = TryGetPrivateRectTransform(board, "baseManaContent");
            if (baseManaContent != null)
            {
                return baseManaContent;
            }

            return TryGetPrivateRectTransform(board, "baseManaParent");
        }
        catch
        {
            return null;
        }
    }

    private static RectTransform TryGetPrivateRectTransform(object instance, string fieldName)
    {
        if (instance == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        try
        {
            Transform transform = Traverse.Create(instance).Field(fieldName).GetValue<Transform>();
            return transform as RectTransform;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 查找默认字体资源（从父容器的现有文本中提取）
    /// </summary>
    /// <param name="root">搜索根节点</param>
    /// <returns>找到的TMP_FontAsset，如果未找到则返回null</returns>
    private static TMP_FontAsset FindDefaultFont(Transform root)
    {
        try
        {
            // 查找第一个TextMeshProUGUI组件并获取其字体
            TextMeshProUGUI tmp = root.GetComponentInChildren<TextMeshProUGUI>(true);
            return tmp != null ? tmp.font : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 尝试通过反射获取UiManager的指定UI层级
    /// </summary>
    /// <param name="fieldName">字段名称（如 "topLayer"、"topmostLayer"）</param>
    /// <returns>找到的RectTransform的Transform，如果失败则返回null</returns>
    private static Transform TryGetUiLayerTransform(string fieldName)
    {
        try
        {
            // 使用Harmony的Traverse来访问私有字段
            RectTransform rect = Traverse.Create(UiManager.Instance).Field(fieldName).GetValue<RectTransform>();
            return rect != null ? rect.transform : null;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region 资源缓存

    /// <summary>缓存的白色Sprite</summary>
    private static Sprite _whiteSprite;

    /// <summary>缓存的白色纹理</summary>
    private static Texture2D _whiteTexture;

    /// <summary>
    /// 获取白色Sprite（用作背景和按钮图像）
    /// 首次调用时创建，之后从缓存返回
    /// </summary>
    /// <returns>白色Sprite</returns>
    private static Sprite GetWhiteSprite()
    {
        // 如果已缓存，直接返回
        if (_whiteSprite != null)
        {
            return _whiteSprite;
        }

        // 创建1x1的白色纹理
        _whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _whiteTexture.SetPixel(0, 0, Color.white);
        // Apply(updateMipmaps=false, makeNoLongerReadable=true)：不生成Mip，释放内存
        _whiteTexture.Apply(false, true);

        // 从纹理创建Sprite
        _whiteSprite = Sprite.Create(_whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _whiteSprite;
    }

    #endregion

    #region 数据模型

    /// <summary>
    /// 玩家摘要信息（用于显示在Overlay中）
    /// </summary>
    private sealed class PlayerSummary
    {
        /// <summary>玩家ID（唯一标识符）</summary>
        public string PlayerId { get; set; }

        /// <summary>玩家昵称</summary>
        public string PlayerName { get; set; }

        /// <summary>是否为房主</summary>
        public bool IsHost { get; set; }

        /// <summary>是否在线连接</summary>
        public bool IsConnected { get; set; }

        /// <summary>角色/模型标识（用于头像/模型加载）</summary>
        public string CharacterId { get; set; }

        /// <summary>地图节点 X（未知为 -1）</summary>
        public int LocationX { get; set; }

        /// <summary>地图节点 Y（未知为 -1）</summary>
        public int LocationY { get; set; }

        /// <summary>当前章节（未知为 -1）</summary>
        public int Stage { get; set; }

        /// <summary>位置名称（如节点类型字符串）</summary>
        public string LocationName { get; set; }

        /// <summary>最后更新时间（用于检测玩家心跳）</summary>
        public float LastUpdateTime { get; set; }
    }

    /// <summary>
    /// Overlay UI根容器的数据结构
    /// </summary>
    private sealed class OverlayUi
    {
        public GameObject Root { get; set; }
        public RectTransform RootRect { get; set; }
        public RectTransform EntriesRoot { get; set; }
        public GameObject AvatarTemplate { get; set; }
        public Dictionary<string, AvatarEntryUi> Entries { get; set; }
    }

    private sealed class AvatarEntryUi
    {
        public string PlayerId { get; set; }
        public GameObject Root { get; set; }
        public RectTransform RootRect { get; set; }
        public Image Avatar { get; set; }
        public TextMeshProUGUI Name { get; set; }
        public TextMeshProUGUI Status { get; set; }
    }
}
    #endregion
