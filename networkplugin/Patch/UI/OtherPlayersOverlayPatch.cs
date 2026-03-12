using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.NetworkPlayer;
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

    private const float AvatarEntryBaseWidth = 260f;
    private const float AvatarEntryBaseHeight = 224f;
    private const float AvatarVisualSize = 112f;
    private const float AvatarEntrySpacing = 12f;
    private const float OverlayRootWidth = 280f;
    private const float OverlayRootTopPadding = 8f;
    private const float OverlayRootBottomPadding = 12f;
    private const float AvatarPanelScale = 0.7f;
    private const float AvatarPanelOffsetY = -10f;
    private const float SkillImageOffsetX = -1f;
    private const float SkillImageOffsetY = -5f;
    private const float SkillImageScale = 1.03f;
    private static readonly Vector2 BackgroundAnchoredPosition = new(0f, -2f);
    private static readonly Vector3 BackgroundLocalScale = new(0.92f, 0.92f, 1f);
    private static readonly Vector2 HealthBarAnchoredPosition = new(0f, 54f);
    private static readonly Vector3 HealthBarLocalScale = new(0.39f, 0.39f, 1f);
    private const float RuntimeLayoutEpsilon = 0.01f;

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
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 1f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(OverlayRootWidth, AvatarEntryBaseHeight + OverlayRootTopPadding + OverlayRootBottomPadding);

        GameObject entriesRootGo = new("EntriesRoot");
        entriesRootGo.transform.SetParent(root.transform, false);
        RectTransform entriesRect = entriesRootGo.AddComponent<RectTransform>();
        entriesRect.anchorMin = new Vector2(1f, 1f);
        entriesRect.anchorMax = new Vector2(1f, 1f);
        entriesRect.pivot = new Vector2(1f, 1f);
        entriesRect.anchoredPosition = new Vector2(0f, -OverlayRootTopPadding);
        entriesRect.sizeDelta = new Vector2(OverlayRootWidth, AvatarEntryBaseHeight);

        _ui = new OverlayUi
        {
            Root = root,
            RootRect = rootRect,
            EntriesRoot = entriesRect,
            RootLayoutState = CaptureRectLayoutState(rootRect),
            EntriesRootLayoutState = CaptureRectLayoutState(entriesRect),
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

        if (!TryAttachUiToTopRight(list.Count))
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

        EnsureHealthBarTemplate();

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

        List<AvatarEntryUi> orderedEntries = new List<AvatarEntryUi>(list.Count);
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

        if (!TryAttachUiToTopRight(orderedEntries.Count))
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
            graphic?.raycastTarget = false;
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
            SetUltimateVisualFieldActive(panel, "powerText", true);
            SetUltimateVisualFieldActive(panel, "gauge1", true);
            SetUltimateVisualFieldActive(panel, "gauge2", true);
            SetUltimateVisualFieldActive(panel, "gauge3", true);
            SetUltimateVisualFieldActive(panel, "fireParticle1", false);
            SetUltimateVisualFieldActive(panel, "fireParticle2", false);
            SetUltimateVisualFieldActive(panel, "fireParticle3", false);
            SetUltimateVisualFieldActive(panel, "lightParticle", false);
        }

        ConfigureAvatarTemplateVisuals(template);

        CanvasGroup group = GetOrAddCanvasGroup(template);
        if (group != null)
        {
            group.alpha = 1f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    private static void ConfigureAvatarTemplateVisuals(GameObject template)
    {
        if (template == null)
        {
            return;
        }

        Transform bg = template.transform.Find("Root/Bg");
        Transform skillImage = template.transform.Find("Root/SkillImage");

        if (bg != null)
        {
            bg.SetAsFirstSibling();
            SetGraphicTreeAlpha(bg, 1f);

            if (bg is RectTransform bgRect)
            {
                bgRect.anchoredPosition = BackgroundAnchoredPosition;
                bgRect.localScale = BackgroundLocalScale;
            }
        }

        if (skillImage != null)
        {
            skillImage.SetAsLastSibling();
            SetGraphicTreeAlpha(skillImage, 1f);

            if (skillImage is RectTransform skillImageRect)
            {
                skillImageRect.anchoredPosition = new Vector2(SkillImageOffsetX, SkillImageOffsetY);
                skillImageRect.localScale = new Vector3(SkillImageScale, SkillImageScale, 1f);
            }
        }
    }

    private static void SetGraphicTreeAlpha(Transform root, float alpha)
    {
        if (root == null)
        {
            return;
        }

        foreach (Graphic graphic in root.GetComponentsInChildren<Graphic>(true))
        {
            if (graphic == null)
            {
                continue;
            }

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }

    private static void SetUltimateVisualFieldActive(UltimateSkillPanel panel, string fieldName, bool active)
    {
        if (panel == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return;
        }

        try
        {
            Component component = Traverse.Create(panel).Field(fieldName).GetValue<Component>();
            component?.gameObject.SetActive(active);
        }
        catch
        {
            // ignored
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
            component?.gameObject.SetActive(false);
        }
        catch
        {
            // ignored
        }
    }

    private static void EnsureHealthBarTemplate()
    {
        if (_ui == null || _ui.EntriesRoot == null || _ui.HealthBarTemplate != null)
        {
            return;
        }

        UnitStatusWidget sourceWidget = null;
        bool destroySource = false;

        try
        {
            UnitStatusHud hud = UiManager.GetPanel<UnitStatusHud>();
            if (hud != null)
            {
                sourceWidget = Traverse.Create(hud).Field("statusTemplate").GetValue<UnitStatusWidget>();
                if (sourceWidget == null && GameDirector.Player != null && GameDirector.Player.Unit != null)
                {
                    sourceWidget = hud.CreateStatusWidget(GameDirector.Player.Unit);
                    destroySource = sourceWidget != null;
                }
            }
        }
        catch
        {
            // ignored
        }

        if (sourceWidget == null)
        {
            return;
        }

        GameObject template = UnityEngine.Object.Instantiate(sourceWidget.gameObject, _ui.EntriesRoot, false);
        template.name = "RemotePlayerHealthBarTemplate";
        PrepareHealthBarTemplate(template);
        template.SetActive(false);
        _ui.HealthBarTemplate = template;

        if (destroySource)
        {
            UnityEngine.Object.Destroy(sourceWidget.gameObject);
        }
    }

    private static void PrepareHealthBarTemplate(GameObject template)
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

        CanvasGroup group = GetOrAddCanvasGroup(template);
        if (group != null)
        {
            group.alpha = 1f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        RectTransform rect = template.GetComponent<RectTransform>();
        ConfigureHealthBarRect(rect);

        UnitStatusWidget widget = template.GetComponent<UnitStatusWidget>();
        if (widget == null)
        {
            return;
        }

        widget.enabled = false;
        widget.Unit = null;

        Transform statusEffectParent = TryGetUnitStatusTransform(widget, "statusEffectParent");
        if (statusEffectParent != null)
        {
            statusEffectParent.gameObject.SetActive(false);
        }

        HealthBar hpBar = TryGetUnitStatusField<HealthBar>(widget, "hpBar");
        if (hpBar != null)
        {
            widget.SetPlayerHpBarLength(60);
            hpBar.TweenHp(60, 60, 0, 0, true);
        }
    }

    private static void ConfigureHealthBarRect(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = HealthBarAnchoredPosition;
        rect.localScale = HealthBarLocalScale;
        rect.localEulerAngles = Vector3.zero;
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
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 1f);
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
        visualRect.anchoredPosition = new Vector2(0f, AvatarPanelOffsetY);
        visualRect.localScale = new Vector3(AvatarPanelScale, AvatarPanelScale, 1f);

        Image avatar = TryGetAvatarImageFromTemplate(visual) ?? CreateFallbackAvatarVisual(visual.transform);
        avatar.raycastTarget = false;
        avatar.preserveAspect = true;

        UltimateSkillPanel panel = visual.GetComponent<UltimateSkillPanel>();
        TextMeshProUGUI powerText = TryGetUltimateField<TextMeshProUGUI>(panel, "powerText");

        TextMeshProUGUI name = CreateTmpText(root.transform, "Name", playerId, 15f);
        name.alignment = TextAlignmentOptions.Center;
        RectTransform nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 0f);
        nameRect.pivot = new Vector2(0.5f, 0f);
        nameRect.anchoredPosition = new Vector2(0f, 30f);
        nameRect.sizeDelta = new Vector2(-8f, 20f);
        name.enableWordWrapping = false;
        name.overflowMode = TextOverflowModes.Ellipsis;

        TextMeshProUGUI status = CreateTmpText(root.transform, "Status", "在线", 13f);
        status.alignment = TextAlignmentOptions.Center;
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(0.5f, 0f);
        statusRect.anchoredPosition = new Vector2(0f, 10f);
        statusRect.sizeDelta = new Vector2(-8f, 18f);
        status.enableWordWrapping = false;
        status.overflowMode = TextOverflowModes.Ellipsis;

        GameObject healthRoot = null;
        RectTransform healthRootRect = null;
        UnitStatusWidget healthWidget = null;
        HealthBar healthBar = null;
        CanvasGroup healthGroup = null;

        if (_ui.HealthBarTemplate != null)
        {
            healthRoot = UnityEngine.Object.Instantiate(_ui.HealthBarTemplate, root.transform, false);
            healthRoot.name = "RemotePlayerHealthBar";
            healthRoot.SetActive(true);
            healthRootRect = healthRoot.GetComponent<RectTransform>();
            ConfigureHealthBarRect(healthRootRect);
            healthWidget = healthRoot.GetComponent<UnitStatusWidget>();
            healthBar = TryGetUnitStatusField<HealthBar>(healthWidget, "hpBar");
            healthGroup = GetOrAddCanvasGroup(healthRoot);
        }

        AvatarEntryUi entry = new AvatarEntryUi
        {
            PlayerId = playerId,
            Root = root,
            RootRect = rootRect,
            RootLayoutState = CaptureRectLayoutState(rootRect),
            Avatar = avatar,
            Name = name,
            Status = status,
            VisualGroup = GetOrAddCanvasGroup(visual),
            PowerText = powerText,
            Gauge1 = TryGetUltimateField<Image>(panel, "gauge1"),
            Gauge2 = TryGetUltimateField<Image>(panel, "gauge2"),
            Gauge3 = TryGetUltimateField<Image>(panel, "gauge3"),
            Gauge1FontColor = TryGetUltimateColorField(panel, "gauge1FontColor", Color.white),
            Gauge2FontColor = TryGetUltimateColorField(panel, "gauge2FontColor", Color.white),
            Gauge3FontColor = TryGetUltimateColorField(panel, "gauge3FontColor", Color.white),
            HealthRoot = healthRoot,
            HealthRootRect = healthRootRect,
            HealthWidget = healthWidget,
            HealthBar = healthBar,
            HealthGroup = healthGroup,
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
            entry.Status.text = BuildStatusText(player.PlayerId, isConnected);
            entry.Status.color = isConnected ? new Color(0.72f, 1f, 0.72f, 1f) : new Color(1f, 0.66f, 0.66f, 1f);
        }

        if (entry.Avatar != null)
        {
            entry.Avatar.sprite = TryGetAvatarSprite(player.CharacterId) ?? GetWhiteSprite();
            entry.Avatar.color = isConnected ? Color.white : new Color(0.55f, 0.55f, 0.55f, 0.95f);
        }

        ApplyBattleState(entry, player, isConnected);
    }

    private static void ApplyBattleState(AvatarEntryUi entry, PlayerSummary player, bool isConnected)
    {
        bool hasBattleState = TryGetRemoteBattleState(player, out RemoteBattleState battleState);

        if (entry.VisualGroup != null)
        {
            entry.VisualGroup.alpha = 1f;
        }

        if (entry.HealthGroup != null)
        {
            entry.HealthGroup.alpha = 1f;
        }

        if (hasBattleState)
        {
            ApplyPowerCharge(entry, battleState, isConnected);
            ApplyHealthBar(entry, battleState);
            return;
        }

        if (entry.HealthRoot != null)
        {
            entry.HealthRoot.SetActive(false);
        }

        ClearPowerCharge(entry);
    }

    private static void ApplyPowerCharge(AvatarEntryUi entry, RemoteBattleState battleState, bool isConnected)
    {
        if (entry == null)
        {
            return;
        }

        int powerPerLevel = Mathf.Max(1, battleState.PowerPerLevel <= 0 ? 1 : battleState.PowerPerLevel);
        int maxPowerLevel = Mathf.Max(1, battleState.MaxPowerLevel <= 0 ? 3 : battleState.MaxPowerLevel);
        int maxPower = powerPerLevel * maxPowerLevel;
        int currentPower = Mathf.Clamp(battleState.CurrentPower, 0, maxPower);

        if (entry.LastCurrentPower == currentPower &&
            entry.LastPowerPerLevel == powerPerLevel &&
            entry.LastMaxPowerLevel == maxPowerLevel)
        {
            return;
        }

        int currentLevel = Mathf.Clamp(currentPower / powerPerLevel, 0, 3);
        int residue = currentPower % powerPerLevel;

        if (entry.Gauge1 != null)
        {
            entry.Gauge1.fillAmount = currentLevel == 0 ? (float)residue / powerPerLevel : 1f;
        }

        if (entry.Gauge2 != null)
        {
            entry.Gauge2.fillAmount = currentLevel switch
            {
                0 => 0f,
                1 => (float)residue / powerPerLevel,
                _ => 1f,
            };
        }

        if (entry.Gauge3 != null)
        {
            entry.Gauge3.fillAmount = currentLevel switch
            {
                0 => 0f,
                1 => 0f,
                2 => (float)residue / powerPerLevel,
                _ => 1f,
            };
        }

        if (entry.PowerText != null)
        {
            Color textColor = currentLevel switch
            {
                1 => entry.Gauge1FontColor,
                2 => entry.Gauge2FontColor,
                3 => entry.Gauge3FontColor,
                _ => Color.white,
            };

            if (!isConnected)
            {
                textColor = Color.Lerp(textColor, new Color(0.74f, 0.74f, 0.74f, 1f), 0.5f);
            }

            entry.PowerText.text = battleState.PowerPerLevel > 0
                ? $"<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{currentPower} </color>/ {powerPerLevel}"
                : "-- / --";
        }

        entry.LastCurrentPower = currentPower;
        entry.LastPowerPerLevel = powerPerLevel;
        entry.LastMaxPowerLevel = maxPowerLevel;
    }

    private static void ClearPowerCharge(AvatarEntryUi entry)
    {
        if (entry == null)
        {
            return;
        }

        if (entry.Gauge1 != null)
        {
            entry.Gauge1.fillAmount = 0f;
        }

        if (entry.Gauge2 != null)
        {
            entry.Gauge2.fillAmount = 0f;
        }

        if (entry.Gauge3 != null)
        {
            entry.Gauge3.fillAmount = 0f;
        }

        if (entry.PowerText != null)
        {
            entry.PowerText.text = "-- / --";
        }

        entry.LastCurrentPower = int.MinValue;
        entry.LastPowerPerLevel = int.MinValue;
        entry.LastMaxPowerLevel = int.MinValue;
    }

    private static void ApplyHealthBar(AvatarEntryUi entry, RemoteBattleState battleState)
    {
        if (entry?.HealthBar == null || entry.HealthWidget == null)
        {
            return;
        }

        if (entry.HealthRoot != null && !entry.HealthRoot.activeSelf)
        {
            entry.HealthRoot.SetActive(true);
        }

        int maxHealth = Mathf.Max(1, battleState.MaxHealth);
        int health = Mathf.Clamp(battleState.Health, 0, maxHealth);
        int shield = Mathf.Max(0, battleState.Shield);
        int block = Mathf.Max(0, battleState.Block);

        if (entry.LastHealth == health && entry.LastMaxHealth == maxHealth && entry.LastShield == shield && entry.LastBlock == block)
        {
            return;
        }

        entry.HealthWidget.SetPlayerHpBarLength(Mathf.Min(maxHealth, 60));
        entry.HealthBar.TweenHp(health, maxHealth, shield, block, !entry.HasInitializedHealthBar);

        entry.LastHealth = health;
        entry.LastMaxHealth = maxHealth;
        entry.LastShield = shield;
        entry.LastBlock = block;
        entry.HasInitializedHealthBar = true;
    }

    private static void LayoutAvatarEntries(List<AvatarEntryUi> entries)
    {
        if (_ui?.EntriesRoot == null || entries == null || entries.Count == 0)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            AvatarEntryUi entry = entries[i];
            if (entry?.RootRect == null)
            {
                continue;
            }

            ApplyRuntimeEditableRectLayout(entry.RootRect, entry.RootLayoutState, rect =>
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.sizeDelta = new Vector2(AvatarEntryBaseWidth, AvatarEntryBaseHeight);
                rect.localScale = Vector3.one;
                rect.anchoredPosition = new Vector2(0f, -i * (AvatarEntryBaseHeight + AvatarEntrySpacing));
            });
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
        GameObject go = new GameObject(name);
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

    private static bool TryAttachUiToTopRight(int entryCount)
    {
        if (_ui?.RootRect == null)
        {
            return false;
        }

        Transform fallback = TryGetUiLayerTransform("topLayer") ?? TryGetUiLayerTransform("topmostLayer") ?? UiManager.Instance?.transform;
        RectTransform parentRect = fallback as RectTransform;
        if (parentRect == null)
        {
            return false;
        }

        if (_ui.RootRect.parent != parentRect)
        {
            _ui.RootRect.SetParent(parentRect, false);
        }

        (float offsetX, float offsetY) = GetOverlayRightOffsets();

        float height = entryCount * AvatarEntryBaseHeight + Mathf.Max(0, entryCount - 1) * AvatarEntrySpacing + OverlayRootTopPadding + OverlayRootBottomPadding;

        ApplyRuntimeEditableRectLayout(_ui.RootRect, _ui.RootLayoutState, rect =>
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(offsetX, offsetY);
            rect.sizeDelta = new Vector2(OverlayRootWidth, height);
        });

        if (_ui.EntriesRoot != null)
        {
            ApplyRuntimeEditableRectLayout(_ui.EntriesRoot, _ui.EntriesRootLayoutState, rect =>
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(0f, -OverlayRootTopPadding);
                rect.sizeDelta = new Vector2(OverlayRootWidth, height - OverlayRootTopPadding - OverlayRootBottomPadding);
            });
        }

        return true;
    }

    private static (float X, float Y) GetOverlayRightOffsets()
    {
        try
        {
            ConfigManager cfg = TryGetConfig();
            float x = cfg?.OtherPlayersOverlayRightOffsetX?.Value ?? -24f;
            float y = cfg?.OtherPlayersOverlayRightOffsetY?.Value ?? -144f;
            return (x, y);
        }
        catch
        {
            return (-24f, -144f);
        }
    }

    private static string BuildStatusText(string playerId, bool isConnected)
    {
        if (!isConnected)
        {
            return "离线";
        }

        PlayerSummary summary = null;
        lock (_syncLock)
        {
            _players.TryGetValue(playerId, out summary);
        }

        if (TryGetRemoteBattleState(summary, out RemoteBattleState battleState))
        {
            var parts = new List<string> { "在线", $"{Mathf.Max(0, battleState.Health)}/{Mathf.Max(1, battleState.MaxHealth)}" };
            int defense = Mathf.Max(0, battleState.Block) + Mathf.Max(0, battleState.Shield);
            if (defense > 0)
            {
                parts.Add($"盾{defense}");
            }

            if (battleState.PowerPerLevel > 0)
            {
                parts.Add($"符{Mathf.Max(0, battleState.CurrentPower)}");
            }

            return string.Join(" · ", parts);
        }

        INetworkPlayer player = TryGetNetworkManager()?.GetPlayer(playerId);
        if (player == null)
        {
            return "在线";
        }

        try
        {
            var parts = new List<string> { "在线" };

            if (player.maxHP > 0)
            {
                parts.Add($"{Mathf.Max(0, player.HP)}/{player.maxHP}");
            }

            int defense = Mathf.Max(0, player.block) + Mathf.Max(0, player.shield);
            if (defense > 0)
            {
                parts.Add($"盾{defense}");
            }

            if (player.endturn)
            {
                parts.Add("已结束");
            }

            return string.Join(" · ", parts);
        }
        catch
        {
            return "在线";
        }
    }

    private static INetworkManager TryGetNetworkManager()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkManager>();
        }
        catch
        {
            return null;
        }
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
            return tmp?.font;
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
            return rect?.transform;
        }
        catch
        {
            return null;
        }
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return null;
        }

        CanvasGroup group = gameObject.GetComponent<CanvasGroup>();
        return group ?? gameObject.AddComponent<CanvasGroup>();
    }

    private static T TryGetUltimateField<T>(UltimateSkillPanel panel, string fieldName) where T : class
    {
        if (panel == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        try
        {
            return Traverse.Create(panel).Field(fieldName).GetValue<T>();
        }
        catch
        {
            return null;
        }
    }

    private static Color TryGetUltimateColorField(UltimateSkillPanel panel, string fieldName, Color fallback)
    {
        if (panel == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return fallback;
        }

        try
        {
            return Traverse.Create(panel).Field(fieldName).GetValue<Color>();
        }
        catch
        {
            return fallback;
        }
    }

    private static T TryGetUnitStatusField<T>(UnitStatusWidget widget, string fieldName) where T : class
    {
        if (widget == null || string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        try
        {
            return Traverse.Create(widget).Field(fieldName).GetValue<T>();
        }
        catch
        {
            return null;
        }
    }

    private static Transform TryGetUnitStatusTransform(UnitStatusWidget widget, string fieldName)
    {
        return TryGetUnitStatusField<Transform>(widget, fieldName);
    }

    private static RectLayoutState CaptureRectLayoutState(RectTransform rect)
    {
        if (rect == null)
        {
            return new RectLayoutState();
        }

        return new RectLayoutState
        {
            HasApplied = true,
            AnchorMin = rect.anchorMin,
            AnchorMax = rect.anchorMax,
            Pivot = rect.pivot,
            AnchoredPosition = rect.anchoredPosition,
            SizeDelta = rect.sizeDelta,
            LocalScale = rect.localScale,
            LocalEulerAngles = rect.localEulerAngles,
        };
    }

    private static void ApplyRuntimeEditableRectLayout(RectTransform rect, RectLayoutState state, Action<RectTransform> apply)
    {
        if (rect == null || state == null || apply == null)
        {
            return;
        }

        if (state.ManualOverride)
        {
            return;
        }

        if (state.HasApplied && HasRectLayoutDrifted(rect, state))
        {
            state.ManualOverride = true;
            return;
        }

        apply(rect);
        UpdateRectLayoutState(rect, state);
    }

    private static void UpdateRectLayoutState(RectTransform rect, RectLayoutState state)
    {
        if (rect == null || state == null)
        {
            return;
        }

        state.HasApplied = true;
        state.AnchorMin = rect.anchorMin;
        state.AnchorMax = rect.anchorMax;
        state.Pivot = rect.pivot;
        state.AnchoredPosition = rect.anchoredPosition;
        state.SizeDelta = rect.sizeDelta;
        state.LocalScale = rect.localScale;
        state.LocalEulerAngles = rect.localEulerAngles;
    }

    private static bool HasRectLayoutDrifted(RectTransform rect, RectLayoutState state)
    {
        if (rect == null || state == null || !state.HasApplied)
        {
            return false;
        }

        return !Approximately(rect.anchorMin, state.AnchorMin) ||
               !Approximately(rect.anchorMax, state.AnchorMax) ||
               !Approximately(rect.pivot, state.Pivot) ||
               !Approximately(rect.anchoredPosition, state.AnchoredPosition) ||
               !Approximately(rect.sizeDelta, state.SizeDelta) ||
               !Approximately(rect.localScale, state.LocalScale) ||
               !Approximately(rect.localEulerAngles, state.LocalEulerAngles);
    }

    private static bool Approximately(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(a.x - b.x) <= RuntimeLayoutEpsilon && Mathf.Abs(a.y - b.y) <= RuntimeLayoutEpsilon;
    }

    private static bool Approximately(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x - b.x) <= RuntimeLayoutEpsilon &&
               Mathf.Abs(a.y - b.y) <= RuntimeLayoutEpsilon &&
               Mathf.Abs(a.z - b.z) <= RuntimeLayoutEpsilon;
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
        public RectLayoutState RootLayoutState { get; set; }
        public RectLayoutState EntriesRootLayoutState { get; set; }
        public GameObject AvatarTemplate { get; set; }
        public GameObject HealthBarTemplate { get; set; }
        public Dictionary<string, AvatarEntryUi> Entries { get; set; }
    }

    private sealed class RectLayoutState
    {
        public bool HasApplied { get; set; }
        public bool ManualOverride { get; set; }
        public Vector2 AnchorMin { get; set; }
        public Vector2 AnchorMax { get; set; }
        public Vector2 Pivot { get; set; }
        public Vector2 AnchoredPosition { get; set; }
        public Vector2 SizeDelta { get; set; }
        public Vector3 LocalScale { get; set; }
        public Vector3 LocalEulerAngles { get; set; }
    }

    private sealed class AvatarEntryUi
    {
        public string PlayerId { get; set; }
        public GameObject Root { get; set; }
        public RectTransform RootRect { get; set; }
        public RectLayoutState RootLayoutState { get; set; }
        public Image Avatar { get; set; }
        public TextMeshProUGUI Name { get; set; }
        public TextMeshProUGUI Status { get; set; }
        public CanvasGroup VisualGroup { get; set; }
        public TextMeshProUGUI PowerText { get; set; }
        public Image Gauge1 { get; set; }
        public Image Gauge2 { get; set; }
        public Image Gauge3 { get; set; }
        public Color Gauge1FontColor { get; set; }
        public Color Gauge2FontColor { get; set; }
        public Color Gauge3FontColor { get; set; }
        public GameObject HealthRoot { get; set; }
        public RectTransform HealthRootRect { get; set; }
        public UnitStatusWidget HealthWidget { get; set; }
        public HealthBar HealthBar { get; set; }
        public CanvasGroup HealthGroup { get; set; }
        public int LastCurrentPower { get; set; } = int.MinValue;
        public int LastPowerPerLevel { get; set; } = int.MinValue;
        public int LastMaxPowerLevel { get; set; } = int.MinValue;
        public int LastHealth { get; set; } = int.MinValue;
        public int LastMaxHealth { get; set; } = int.MinValue;
        public int LastShield { get; set; } = int.MinValue;
        public int LastBlock { get; set; } = int.MinValue;
        public bool HasInitializedHealthBar { get; set; }
    }
}
    #endregion
