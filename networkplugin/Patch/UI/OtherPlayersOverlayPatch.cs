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
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
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
public static class OtherPlayersOverlayPatch
{
    #region 常量和字段

    /// <summary>每页显示的玩家数量</summary>
    private const int PageSize = 10;

    /// <summary>获取依赖注入容器</summary>
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    /// <summary>用于同步访问玩家列表的锁</summary>
    private static readonly object _syncLock = new();

    /// <summary>存储所有玩家信息的字典，key为PlayerId</summary>
    private static readonly Dictionary<string, PlayerSummary> _players = new();

    /// <summary>当前页码</summary>
    private static int _page;

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
                // 没有网络客户端时隐藏 UI 并直接返回
                HideUi();
                HideRemoteCharacters();
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
                HideUi();
                HideRemoteCharacters();
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

            // 确保 Overlay UI 已创建并处于激活状态
            EnsureUi();
            // 根据当前玩家列表和页码刷新 UI 内容
            RefreshUi();

            // 渲染远程玩家“角色实体”（战斗场景）
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

    #region 网络客户端操作

    /// <summary>
    /// 尝试从依赖注入容器获取网络客户端实例
    /// </summary>
    /// <returns>网络客户端实例，如果获取失败则返回null</returns>
    private static INetworkClient TryGetNetworkClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[OtherPlayersOverlayPatch] 获取 INetworkClient 失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 确保已订阅网络客户端的事件
    /// 如果客户端实例变更，会自动重新订阅
    /// </summary>
    /// <param name="client">网络客户端实例</param>
    private static void EnsureSubscribed(INetworkClient client)
    {
        // 如果已订阅相同的客户端，无需重复订阅
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
        {
            // 已是同一实例，保持现有订阅
            return;
        }

        // 取消旧客户端的事件订阅
        try
        {
            if (_subscribedClient != null)
            {
                // 卸载旧客户端的回调，避免重复触发
                _subscribedClient.OnGameEventReceived -= _onGameEventReceived;
                _subscribedClient.OnConnectionStateChanged -= _onConnectionStateChanged;
            }
        }
        catch
        {
            // 忽略：旧客户端可能已被销毁/异常
        }

        // 订阅新客户端的事件
        try
        {
            // 注册新客户端的事件回调
            client.OnGameEventReceived += _onGameEventReceived;
            client.OnConnectionStateChanged += _onConnectionStateChanged;
            // 记录当前订阅状态
            _subscribedClient = client;
            _subscribed = true;
        }
        catch (Exception ex)
        {
            // 订阅失败时重置状态并记录日志
            Plugin.Logger?.LogWarning($"[OtherPlayersOverlayPatch] 订阅网络事件失败: {ex.Message}");
            _subscribed = false;
            _subscribedClient = null;
        }
    }

    #endregion

    #region 网络事件处理

    /// <summary>
    /// 处理网络连接状态变化事件
    /// 当连接断开时，清空玩家列表并重置页码
    /// </summary>
    /// <param name="connected">是否已连接</param>
    private static void OnConnectionStateChanged(bool connected)
    {
        // 仅处理断开连接的情况
        if (connected)
        {
            return;
        }

        lock (_syncLock)
        {
            // 清空玩家列表
            _players.Clear();
            // 重置页码
            _page = 0;
        }

        _selfPlayerId = null;
        ClearRemoteCharacters();
        ClearMapIcons();
    }

    /// <summary>
    /// 处理从网络接收到的游戏事件
    /// 支持的事件类型：PlayerListUpdate、PlayerJoined、HostChanged
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="payload">事件负载（JSON格式）</param>
    private static void OnGameEventReceived(string eventType, object payload)
    {
        try
        {
            // 尝试将payload转换为JsonElement
            if (!TryGetJsonElement(payload, out JsonElement root))
            {
                return;
            }

            // 根据事件类型分发处理
            switch (eventType)
            {
                case "Welcome":
                    // 服务器欢迎消息：包含自身 PlayerId 与初始玩家列表
                    HandleWelcome(root);
                    break;
                case "PlayerListUpdate":
                    // 玩家列表更新事件：完整覆盖现有玩家列表
                    HandlePlayerListUpdate(root);
                    break;
                case "PlayerJoined":
                    // 新玩家加入事件：添加新玩家
                    HandlePlayerJoined(root);
                    break;
                case "PlayerLeft":
                    // 玩家离开事件：移除玩家
                    HandlePlayerLeft(root);
                    break;
                case "HostChanged":
                    // 房主变更事件：更新房主标记
                    HandleHostChanged(root);
                    break;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[OtherPlayersOverlayPatch] 处理网络事件失败: {eventType}, {ex.Message}");
        }
    }

    /// <summary>
    /// 处理玩家列表更新事件（完全替换）
    /// 通常在连接时或全量同步时调用
    /// </summary>
    /// <param name="root">JSON数据根元素，应包含 "Players" 数组</param>
    private static void HandlePlayerListUpdate(JsonElement root)
    {
        if (!root.TryGetProperty("Players", out JsonElement playersElem) || playersElem.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        ReplacePlayersFromArray(playersElem);
    }

    /// <summary>
    /// 处理新玩家加入事件
    /// </summary>
    /// <param name="root">JSON数据根元素，应包含 "PlayerId"、"PlayerName"、"IsHost"</param>
    private static void HandlePlayerJoined(JsonElement root)
    {
        string playerId = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        bool hasConnectedField = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("IsConnected", out _);

        lock (_syncLock)
        {
            _players[playerId] = new PlayerSummary
            {
                PlayerId = playerId,
                PlayerName = GetString(root, "PlayerName") ?? playerId,
                IsHost = GetBool(root, "IsHost"),
                IsConnected = hasConnectedField ? GetBool(root, "IsConnected") : true,
                CharacterId = GetString(root, "CharacterId"),
                LocationX = GetInt(root, "LocationX", -1),
                LocationY = GetInt(root, "LocationY", -1),
                Stage = GetInt(root, "Stage", -1),
                LocationName = GetString(root, "LocationName"),
                LastUpdateTime = Time.unscaledTime,
            };

            ClampPage_NoLock();
        }
    }

    /// <summary>
    /// 处理房主变更事件
    /// 更新所有玩家的IsHost标记
    /// </summary>
    /// <param name="root">JSON数据根元素，应包含 "NewHostId"</param>
    private static void HandleHostChanged(JsonElement root)
    {
        // 获取新房主的ID
        string newHostId = GetString(root, "NewHostId");
        if (string.IsNullOrWhiteSpace(newHostId))
        {
            return;
        }

        lock (_syncLock)
        {
            // 遍历所有玩家，更新房主标记
            foreach (KeyValuePair<string, PlayerSummary> kv in _players)
            {
                kv.Value.IsHost = kv.Key == newHostId;
            }
        }
    }

    /// <summary>
    /// 处理 Welcome 消息：记录自身 PlayerId，并应用初始玩家列表
    /// </summary>
    /// <param name="root">JSON 数据根元素，应包含 "PlayerId" 与 "PlayerList"</param>
    private static void HandleWelcome(JsonElement root)
    {
        string selfId = GetString(root, "PlayerId");
        if (!string.IsNullOrWhiteSpace(selfId))
        {
            _selfPlayerId = selfId;
        }

        if (root.TryGetProperty("PlayerList", out JsonElement playersElem) && playersElem.ValueKind == JsonValueKind.Array)
        {
            ReplacePlayersFromArray(playersElem);
        }
    }

    /// <summary>
    /// 处理玩家离开事件：移除玩家并清理对应渲染缓存
    /// </summary>
    /// <param name="root">JSON 数据根元素，应包含 "PlayerId"</param>
    private static void HandlePlayerLeft(JsonElement root)
    {
        string playerId = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (_syncLock)
        {
            _players.Remove(playerId);
            ClampPage_NoLock();
        }

        RemoveRemoteCharacter(playerId);
        HideMapIcon(playerId);
    }

    /// <summary>
    /// 从 JSON 数组解析玩家列表并整体替换本地缓存
    /// </summary>
    private static void ReplacePlayersFromArray(JsonElement playersElem)
    {
        var incoming = new Dictionary<string, PlayerSummary>();
        foreach (JsonElement p in playersElem.EnumerateArray())
        {
            string playerId = GetString(p, "PlayerId");
            if (string.IsNullOrWhiteSpace(playerId))
            {
                continue;
            }

            bool hasConnectedField = p.ValueKind == JsonValueKind.Object && p.TryGetProperty("IsConnected", out _);
            incoming[playerId] = new PlayerSummary
            {
                PlayerId = playerId,
                PlayerName = GetString(p, "PlayerName") ?? playerId,
                IsHost = GetBool(p, "IsHost"),
                IsConnected = hasConnectedField ? GetBool(p, "IsConnected") : true,
                CharacterId = GetString(p, "CharacterId"),
                LocationX = GetInt(p, "LocationX", -1),
                LocationY = GetInt(p, "LocationY", -1),
                Stage = GetInt(p, "Stage", -1),
                LocationName = GetString(p, "LocationName"),
                LastUpdateTime = Time.unscaledTime,
            };
        }

        lock (_syncLock)
        {
            _players.Clear();
            foreach (var kv in incoming)
            {
                _players[kv.Key] = kv.Value;
            }

            ClampPage_NoLock();
        }
    }

    #endregion

    #region UI 创建和更新

    /// <summary>
    /// 确保UI已创建，如果未创建则进行初始化
    /// 包括背景、标题、翻页按钮、玩家条目等
    /// </summary>
    private static void EnsureUi()
    {
        // 如果UI已存在，直接显示并返回
        if (_ui != null && _ui.Root != null)
        {
            _ui.Root.SetActive(true);
            return;
        }

        // 查找合适的UI层级作为父对象（优先使用topLayer或topmostLayer）
        Transform parent = TryGetUiLayerTransform("topLayer") ?? TryGetUiLayerTransform("topmostLayer") ?? UiManager.Instance.transform;
        // 缓存默认字体（为了提高性能）
        _defaultFont ??= FindDefaultFont(parent);

        // 创建根容器
        GameObject root = new("NetworkPlugin_OtherPlayersOverlay");
        root.transform.SetParent(parent, false);

        // 设置根容器的RectTransform（位置在右下角，距离屏幕边缘24像素）
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);  // 锚点：右上角
        rootRect.anchorMax = new Vector2(1f, 1f);  // 锚点：右上角
        rootRect.pivot = new Vector2(1f, 1f);      // 中心点：右上角
        rootRect.anchoredPosition = new Vector2(-24f, -24f);  // 距离锚点偏移
        rootRect.sizeDelta = new Vector2(360f, 520f);  // 宽360，高520

        // 设置背景图像（半透明黑色）
        Image bg = root.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = new Color(0f, 0f, 0f, 0.35f);  // 黑色，35%不透明度
        bg.raycastTarget = false;  // 不阻挡射线检测

        // 创建标题文本（"联机玩家（其他玩家信息框）"）
        TextMeshProUGUI title = CreateTmpText(root.transform, "Title", "联机玩家（其他玩家信息框）", 20);
        title.alignment = TextAlignmentOptions.TopLeft;
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);  // 左上角
        titleRect.anchorMax = new Vector2(1f, 1f);  // 右上角
        titleRect.pivot = new Vector2(0.5f, 1f);   // 中上
        titleRect.anchoredPosition = new Vector2(0f, -10f);  // 距上边10像素
        titleRect.sizeDelta = new Vector2(-20f, 30f);  // 留出左右各10像素的空白

        // 创建上一页按钮（"<"）
        Button leftButton = CreatePageButton(root.transform, "PrevPage", "<");
        RectTransform leftRect = leftButton.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(1f, 1f);
        leftRect.anchorMax = new Vector2(1f, 1f);
        leftRect.pivot = new Vector2(1f, 1f);
        leftRect.anchoredPosition = new Vector2(-66f, -10f);  // 距右边66像素
        leftRect.sizeDelta = new Vector2(24f, 24f);
        // 绑定点击事件：页码减1，并确保不小于0
        leftButton.onClick.AddListener(() =>
        {
            lock (_syncLock)
            {
                _page = Mathf.Max(0, _page - 1);
            }
        });

        // 创建下一页按钮（">"）
        Button rightButton = CreatePageButton(root.transform, "NextPage", ">");
        RectTransform rightRect = rightButton.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(1f, 1f);
        rightRect.anchorMax = new Vector2(1f, 1f);
        rightRect.pivot = new Vector2(1f, 1f);
        rightRect.anchoredPosition = new Vector2(-36f, -10f);  // 距右边36像素（在"<"按钮的右侧）
        rightRect.sizeDelta = new Vector2(24f, 24f);
        // 绑定点击事件：页码加1，并调整到有效范围
        rightButton.onClick.AddListener(() =>
        {
            lock (_syncLock)
            {
                _page += 1;
                ClampPage_NoLock();
            }
        });

        // 创建玩家条目列表（每页显示10个）
        var entries = new List<EntryUi>(PageSize);
        for (int i = 0; i < PageSize; i++)
        {
            entries.Add(CreateEntry(root.transform, i));
        }

        // 保存UI实例以供后续更新使用
        _ui = new OverlayUi
        {
            Root = root,
            Title = title,
            PrevPage = leftButton,
            NextPage = rightButton,
            Entries = entries,
        };
    }

    /// <summary>
    /// 根据当前页码和玩家列表刷新UI显示内容
    /// 包括排序玩家、计算分页、更新标题和条目内容
    /// </summary>
    private static void RefreshUi()
    {
        List<PlayerSummary> list;
        int page;

        lock (_syncLock)
        {
            // 获取玩家列表副本，按照以下规则排序：
            // 1. 房主优先（IsHost为true）
            // 2. 在线玩家优先（IsConnected为true）
            // 3. 按玩家名称字母顺序
            list = _players.Values
                .OrderByDescending(p => p.IsHost)
                .ThenByDescending(p => p.IsConnected)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            page = _page;
        }

        // 如果没有玩家，隐藏UI
        if (list.Count == 0)
        {
            HideUi();
            return;
        }

        // 计算总页数并调整当前页码
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(list.Count / (float)PageSize));
        page = Mathf.Clamp(page, 0, pageCount - 1);
        lock (_syncLock)
        {
            _page = page;
        }

        // 计算当前页的起始和结束索引
        int startIndex = page * PageSize;
        int endIndex = Mathf.Min(startIndex + PageSize, list.Count);
        int visibleCount = Mathf.Max(0, endIndex - startIndex);

        // 更新标题（显示当前页码）
        _ui.Root.SetActive(true);
        _ui.Title.text = $"联机玩家（第 {page + 1}/{pageCount} 页）";

        // 更新翻页按钮的可交互状态
        _ui.PrevPage.interactable = page > 0;           // 有上一页时启用
        _ui.NextPage.interactable = page + 1 < pageCount;  // 有下一页时启用

        // 更新玩家条目显示
        for (int i = 0; i < _ui.Entries.Count; i++)
        {
            bool visible = i < visibleCount;
            _ui.Entries[i].Root.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            // 获取玩家数据并格式化显示
            PlayerSummary p = list[startIndex + i];
            string hostTag = p.IsHost ? " [房主]" : "";
            string connTag = p.IsConnected ? "在线" : "离线";

            _ui.Entries[i].Name.text = $"{p.PlayerName}{hostTag}";
            _ui.Entries[i].Status.text = connTag;

            // 根据连接状态设置背景颜色（离线玩家显示为红色）
            Color baseColor = p.IsConnected ? new Color(0f, 0f, 0f, 0.25f) : new Color(0.2f, 0.0f, 0.0f, 0.25f);
            _ui.Entries[i].Background.color = baseColor;
        }
    }

    /// <summary>
    /// 隐藏UI（将根容器设为非活跃状态）
    /// </summary>
    private static void HideUi()
    {
        if (_ui?.Root != null)
        {
            _ui.Root.SetActive(false);
        }
    }

    /// <summary>
    /// 调整页码，确保不超过总页数范围
    /// 此方法必须在持有 _syncLock 锁时调用
    /// </summary>
    private static void ClampPage_NoLock()
    {
        int count = _players.Count;
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(count / (float)PageSize));
        _page = Mathf.Clamp(_page, 0, pageCount - 1);
    }

    #endregion

    #region UI 控件创建

    /// <summary>
    /// 创建玩家条目UI（包括玩家名称和在线状态）
    /// </summary>
    /// <param name="parent">父容器</param>
    /// <param name="index">条目索引（0-9），用于计算垂直位置</param>
    /// <returns>创建的条目UI对象</returns>
    #region 远程玩家“角色实体”渲染（战斗 + 地图）

    private static void EnsureRemoteCharacters()
    {
        // 仅在战斗场景渲染：需要 PlayerUnitView、UnitStatusHud 等运行时对象
        if (Singleton<GameDirector>.Instance == null || Singleton<GameDirector>.Instance.PlayerUnitView == null)
        {
            HideRemoteCharacters();
            return;
        }

        Transform unitRoot = TryGetGameDirectorTransform("unitRoot");
        Transform playerRoot = TryGetGameDirectorTransform("playerRoot");
        GameObject unitPrefab = TryGetGameDirectorUnitPrefab();
        if (unitRoot == null || playerRoot == null || unitPrefab == null)
        {
            return;
        }

        if (_remoteCharactersRoot == null)
        {
            GameObject rootGo = new("NetworkPlugin_RemoteCharacters");
            rootGo.transform.SetParent(unitRoot, false);
            _remoteCharactersRoot = rootGo.transform;
        }

        List<PlayerSummary> remotePlayers;
        lock (_syncLock)
        {
            remotePlayers = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)
                .OrderByDescending(p => p.IsHost)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (remotePlayers.Count == 0)
        {
            HideRemoteCharacters();
            return;
        }

        _remoteCharactersRoot.gameObject.SetActive(true);

        var alive = new HashSet<string>(remotePlayers.Select(p => p.PlayerId));
        foreach (string existingId in _remoteCharacters.Keys.ToList())
        {
            if (!alive.Contains(existingId))
            {
                RemoveRemoteCharacter(existingId);
            }
        }

        foreach (PlayerSummary p in remotePlayers)
        {
            EnsureRemoteCharacterView(unitPrefab, p);
        }
    }

    private static void UpdateRemoteCharactersLayout()
    {
        if (_remoteCharactersRoot == null || !_remoteCharactersRoot.gameObject.activeInHierarchy)
        {
            return;
        }

        Transform playerRoot = TryGetGameDirectorTransform("playerRoot");
        if (playerRoot == null)
        {
            return;
        }

        List<PlayerSummary> remotePlayers;
        lock (_syncLock)
        {
            remotePlayers = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)
                .OrderByDescending(p => p.IsHost)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        Vector3 basePos = playerRoot.localPosition;
        const float xStep = 1.4f;
        const float yStep = 0.5f;
        const float scale = 0.85f;

        for (int index = 0; index < remotePlayers.Count; index++)
        {
            PlayerSummary p = remotePlayers[index];
            if (!_remoteCharacters.TryGetValue(p.PlayerId, out RemoteCharacterView view) || view?.Root == null)
            {
                continue;
            }

            int row = index / 2;
            int col = index % 2;
            float x = (row + 1) * xStep;
            float y = col == 0 ? yStep : -yStep;

            view.Root.transform.localPosition = basePos + new Vector3(x, y, 0f);
            view.Root.transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private static void TickRemoteCharacters()
    {
        if (_remoteCharacters.Count == 0)
        {
            return;
        }

        foreach (RemoteCharacterView rc in _remoteCharacters.Values)
        {
            if (rc?.View == null || rc.Root == null || !rc.Root.activeInHierarchy)
            {
                continue;
            }

            rc.View.Tick();
        }
    }

    private static void EnsureRemoteCharacterView(GameObject unitPrefab, PlayerSummary player)
    {
        if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
        {
            return;
        }

        string desiredCharacter = player.CharacterId;
        if (string.IsNullOrWhiteSpace(desiredCharacter))
        {
            desiredCharacter = GetFallbackCharacterId();
        }

        if (_remoteCharacters.TryGetValue(player.PlayerId, out RemoteCharacterView existing))
        {
            if (existing != null && existing.Root != null && string.Equals(existing.CharacterId, desiredCharacter, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            RemoveRemoteCharacter(player.PlayerId);
        }

        UnitStatusHud hud = UiManager.GetPanel<UnitStatusHud>();
        if (hud == null)
        {
            return;
        }

        PlayerUnit unit = TryCreatePlayerUnit(desiredCharacter);
        if (unit == null)
        {
            return;
        }

        try
        {
            unit.Initialize();
        }
        catch
        {
            // 忽略初始化失败：仅用于渲染外观
        }

        GameObject container = new($"RemotePlayer_{player.PlayerId}");
        container.transform.SetParent(_remoteCharactersRoot, false);

        GameObject go = UnityEngine.Object.Instantiate(unitPrefab, container.transform);
        UnitView view = go.GetComponent<UnitView>();
        if (view == null)
        {
            UnityEngine.Object.Destroy(container);
            return;
        }

        view.Unit = unit;
        view.SetStatusWidget(hud.CreateStatusWidget(unit), 0f);
        view.SetInfoWidget(hud.CreateInfoWidget(unit), 0f);

        unit.SetView(view);
        view.IsHidden = false;
        DisableRemoteCharacterInteractions(view);

        _remoteCharacters[player.PlayerId] = new RemoteCharacterView
        {
            PlayerId = player.PlayerId,
            CharacterId = desiredCharacter,
            Root = container,
            View = view,
        };

        try
        {
            Singleton<GameDirector>.Instance.StartCoroutine(view.LoadUnitModelAsync(unit.ModelName, true, default(float?)).ToCoroutine());
        }
        catch
        {
            // 忽略：避免加载失败影响主循环
        }
    }

    private static void DisableRemoteCharacterInteractions(UnitView view)
    {
        if (view == null)
        {
            return;
        }

        // 远程玩家“仅渲染”：禁用碰撞/选择框，避免影响本地鼠标选择与受击判定。
        try
        {
            if (view.BoxCollider != null)
            {
                view.BoxCollider.enabled = false;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            Collider2D circle = Traverse.Create(view).Field("_circleCollider").GetValue<Collider2D>();
            if (circle != null)
            {
                circle.enabled = false;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            Collider selector = view.SelectorCollider;
            if (selector != null)
            {
                selector.enabled = false;
                selector.gameObject.SetActive(false);
            }
        }
        catch
        {
            // ignored
        }
    }

    internal static IReadOnlyList<UnitView> SnapshotRemoteCharacterUnitViews()
    {
        try
        {
            if (_remoteCharacters.Count == 0)
            {
                return Array.Empty<UnitView>();
            }

            var list = new List<UnitView>(_remoteCharacters.Count);
            foreach (RemoteCharacterView rc in _remoteCharacters.Values)
            {
                if (rc?.View == null || rc.Root == null || !rc.Root.activeInHierarchy)
                {
                    continue;
                }

                list.Add(rc.View);
            }
            return list;
        }
        catch
        {
            return Array.Empty<UnitView>();
        }
    }

    internal static void SetRemoteCharacterTargetingEnabled(bool enabled)
    {
        try
        {
            if (_remoteCharacters.Count == 0)
            {
                return;
            }

            foreach (RemoteCharacterView rc in _remoteCharacters.Values)
            {
                if (rc?.View == null)
                {
                    continue;
                }

                SetSelectorColliderEnabled(rc.View, enabled);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void SetSelectorColliderEnabled(UnitView view, bool enabled)
    {
        if (view == null)
        {
            return;
        }

        try
        {
            Collider selector = view.SelectorCollider;
            if (selector == null)
            {
                return;
            }

            selector.enabled = enabled;
            selector.gameObject.SetActive(enabled);
        }
        catch
        {
            // ignored
        }
    }

    private static void HideRemoteCharacters()
    {
        if (_remoteCharactersRoot != null)
        {
            _remoteCharactersRoot.gameObject.SetActive(false);
        }
    }

    private static void ClearRemoteCharacters()
    {
        foreach (string playerId in _remoteCharacters.Keys.ToList())
        {
            RemoveRemoteCharacter(playerId);
        }

        if (_remoteCharactersRoot != null)
        {
            UnityEngine.Object.Destroy(_remoteCharactersRoot.gameObject);
            _remoteCharactersRoot = null;
        }
    }

    private static void RemoveRemoteCharacter(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        if (_remoteCharacters.TryGetValue(playerId, out RemoteCharacterView rc))
        {
            if (rc?.Root != null)
            {
                UnityEngine.Object.Destroy(rc.Root);
            }
            _remoteCharacters.Remove(playerId);
        }
    }

    private static Transform TryGetGameDirectorTransform(string fieldName)
    {
        try
        {
            return Traverse.Create(Singleton<GameDirector>.Instance).Field(fieldName).GetValue<Transform>();
        }
        catch
        {
            return null;
        }
    }

    private static GameObject TryGetGameDirectorUnitPrefab()
    {
        try
        {
            return Traverse.Create(Singleton<GameDirector>.Instance).Field("unitPrefab").GetValue<GameObject>();
        }
        catch
        {
            return null;
        }
    }

    private static string GetFallbackCharacterId()
    {
        try
        {
            PlayerUnit local = Singleton<GameDirector>.Instance.PlayerUnitView?.Unit as PlayerUnit;
            return local?.ModelName ?? local?.Id ?? "Koishi";
        }
        catch
        {
            return "Koishi";
        }
    }

    private static PlayerUnit TryCreatePlayerUnit(string characterId)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(characterId))
            {
                PlayerUnit unit = Library.TryCreatePlayerUnit(characterId);
                if (unit != null)
                {
                    return unit;
                }
            }
        }
        catch
        {
            // 忽略：继续 fallback
        }

        try
        {
            return Library.TryCreatePlayerUnit("Koishi") ?? Library.CreatePlayerUnit("Koishi");
        }
        catch
        {
            return null;
        }
    }

    private static void UpdateMapIcons(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return;
        }

        INetworkClient client = TryGetNetworkClient();
        if (client == null || !client.IsConnected)
        {
            HideAllMapIcons();
            return;
        }

        MapNodeWidget[,] widgets;
        RectTransform nodeHolder;
        try
        {
            widgets = Traverse.Create(mapPanel).Field("_mapNodeWidgets").GetValue<MapNodeWidget[,]>();
            nodeHolder = Traverse.Create(mapPanel).Field("nodeHolder").GetValue<RectTransform>();
        }
        catch
        {
            return;
        }

        if (widgets == null || nodeHolder == null)
        {
            return;
        }

        EnsureMapIconsRoot(nodeHolder);
        _defaultFont ??= FindDefaultFont(nodeHolder);

        List<PlayerSummary> players;
        lock (_syncLock)
        {
            players = _players.Values
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.PlayerId))
                .Where(p => p.IsConnected)
                .Where(p => string.IsNullOrWhiteSpace(_selfPlayerId) || p.PlayerId != _selfPlayerId)
                .Where(p => p.LocationX >= 0 && p.LocationY >= 0)
                .ToList();
        }

        if (players.Count == 0)
        {
            HideAllMapIcons();
            return;
        }

        _mapIconsRoot.gameObject.SetActive(true);

        var alive = new HashSet<string>();
        foreach (var group in players.GroupBy(p => (X: p.LocationX, Y: p.LocationY)))
        {
            int x = group.Key.X;
            int y = group.Key.Y;

            if (x < widgets.GetLowerBound(0) || x > widgets.GetUpperBound(0) || y < widgets.GetLowerBound(1) || y > widgets.GetUpperBound(1))
            {
                continue;
            }

            MapNodeWidget widget = widgets[x, y];
            if (widget == null)
            {
                continue;
            }

            int i = 0;
            foreach (PlayerSummary p in group.OrderByDescending(p => p.IsHost).ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase))
            {
                alive.Add(p.PlayerId);

                MapIconUi icon = EnsureMapIcon(p);
                icon.Root.SetActive(true);

                icon.RootRect.localPosition = widget.transform.localPosition + new Vector3(0f, 70f + i * 18f, 0f);
                icon.Label.text = p.PlayerName;
                icon.Image.color = p.IsHost ? new Color(1f, 0.95f, 0.4f, 1f) : Color.white;

                i++;
            }
        }

        foreach (var kv in _mapIcons)
        {
            if (!alive.Contains(kv.Key))
            {
                kv.Value.Root.SetActive(false);
            }
        }
    }

    private static void EnsureMapIconsRoot(RectTransform nodeHolder)
    {
        if (_mapIconsRoot != null && _mapIconsRoot.transform.parent == nodeHolder)
        {
            return;
        }

        ClearMapIcons();

        GameObject root = new("NetworkPlugin_RemotePlayerIcons");
        root.transform.SetParent(nodeHolder, false);

        _mapIconsRoot = root.AddComponent<RectTransform>();
        _mapIconsRoot.anchorMin = Vector2.zero;
        _mapIconsRoot.anchorMax = Vector2.one;
        _mapIconsRoot.offsetMin = Vector2.zero;
        _mapIconsRoot.offsetMax = Vector2.zero;
    }

    private static MapIconUi EnsureMapIcon(PlayerSummary player)
    {
        if (_mapIcons.TryGetValue(player.PlayerId, out MapIconUi ui) && ui?.Root != null)
        {
            if (!string.Equals(ui.CharacterId, player.CharacterId, StringComparison.OrdinalIgnoreCase))
            {
                SetMapIconSprite(ui, player.CharacterId);
            }
            return ui;
        }

        GameObject root = new($"RemoteIcon_{player.PlayerId}");
        root.transform.SetParent(_mapIconsRoot, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(48f, 64f);

        GameObject avatarGo = new("Avatar");
        avatarGo.transform.SetParent(root.transform, false);

        RectTransform avatarRect = avatarGo.AddComponent<RectTransform>();
        avatarRect.anchorMin = new Vector2(0f, 1f);
        avatarRect.anchorMax = new Vector2(1f, 1f);
        avatarRect.pivot = new Vector2(0.5f, 1f);
        avatarRect.anchoredPosition = Vector2.zero;
        avatarRect.sizeDelta = new Vector2(0f, 48f);

        Image avatar = avatarGo.AddComponent<Image>();
        avatar.raycastTarget = false;
        avatar.preserveAspect = true;

        TextMeshProUGUI label = CreateTmpText(root.transform, "Name", player.PlayerName ?? player.PlayerId, 14f);
        label.alignment = TextAlignmentOptions.Center;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 0f);
        labelRect.pivot = new Vector2(0.5f, 0f);
        labelRect.anchoredPosition = Vector2.zero;
        labelRect.sizeDelta = new Vector2(0f, 16f);

        var icon = new MapIconUi
        {
            Root = root,
            RootRect = rootRect,
            Image = avatar,
            Label = label,
            CharacterId = null,
        };

        _mapIcons[player.PlayerId] = icon;
        SetMapIconSprite(icon, player.CharacterId);
        return icon;
    }

    private static void SetMapIconSprite(MapIconUi icon, string characterId)
    {
        icon.CharacterId = characterId;
        icon.Image.sprite = TryGetAvatarSprite(characterId) ?? GetWhiteSprite();
    }

    private static Sprite TryGetAvatarSprite(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        if (_avatarCache.TryGetValue(characterId, out Sprite cached) && cached != null)
        {
            return cached;
        }

        try
        {
            Sprite sprite = ResourcesHelper.LoadCharacterAvatarSprite(characterId);
            if (sprite != null)
            {
                _avatarCache[characterId] = sprite;
            }
            return sprite;
        }
        catch
        {
            return null;
        }
    }

    private static void HideMapIcon(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        if (_mapIcons.TryGetValue(playerId, out MapIconUi icon) && icon?.Root != null)
        {
            icon.Root.SetActive(false);
        }
    }

    private static void HideAllMapIcons()
    {
        if (_mapIconsRoot != null)
        {
            _mapIconsRoot.gameObject.SetActive(false);
        }
    }

    private static void ClearMapIcons()
    {
        foreach (MapIconUi icon in _mapIcons.Values)
        {
            if (icon?.Root != null)
            {
                UnityEngine.Object.Destroy(icon.Root);
            }
        }
        _mapIcons.Clear();

        if (_mapIconsRoot != null)
        {
            UnityEngine.Object.Destroy(_mapIconsRoot.gameObject);
            _mapIconsRoot = null;
        }
    }

    private sealed class RemoteCharacterView
    {
        public string PlayerId { get; set; }
        public string CharacterId { get; set; }
        public GameObject Root { get; set; }
        public UnitView View { get; set; }
    }

    private sealed class MapIconUi
    {
        public GameObject Root { get; set; }
        public RectTransform RootRect { get; set; }
        public Image Image { get; set; }
        public TextMeshProUGUI Label { get; set; }
        public string CharacterId { get; set; }
    }

    #endregion

    private static EntryUi CreateEntry(Transform parent, int index)
    {
        const float entryHeight = 42f;
        const float topOffset = 48f;
        const float leftPadding = 12f;
        const float rightPadding = 12f;

        // 创建条目容器（命名为 Entry_00、Entry_01 等）
        var go = new GameObject($"Entry_{index:00}");
        go.transform.SetParent(parent, false);

        // 配置条目的RectTransform（纵向排列，每个条目高42像素，间隔6像素）
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);      // 左上角
        rect.anchorMax = new Vector2(1f, 1f);      // 右上角
        rect.pivot = new Vector2(0.5f, 1f);        // 中上
        // 垂直位置：第一个条目距标题48像素，之后每个条目递进 48像素
        rect.anchoredPosition = new Vector2(0f, -(topOffset + index * (entryHeight + 6f)));
        rect.sizeDelta = new Vector2(-24f, entryHeight);  // 宽度留出左右各12像素，高度42

        // 设置条目背景
        Image bg = go.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = new Color(0f, 0f, 0f, 0.25f);  // 浅灰色背景
        bg.raycastTarget = false;  // 不阻挡射线检测

        // 创建玩家名称文本（显示在左侧）
        TextMeshProUGUI name = CreateTmpText(go.transform, "Name", "Player", 18);
        name.alignment = TextAlignmentOptions.Left;
        RectTransform nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = new Vector2(leftPadding, 4f);       // 左侧12像素，上下各4像素
        nameRect.offsetMax = new Vector2(-110f, -4f);            // 右侧留110像素给状态文本

        // 创建在线状态文本（显示在右侧）
        TextMeshProUGUI status = CreateTmpText(go.transform, "Status", "在线", 16);
        status.alignment = TextAlignmentOptions.Right;
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 1f);
        statusRect.offsetMin = new Vector2(160f, 4f);            // 左侧160像素起
        statusRect.offsetMax = new Vector2(-rightPadding, -4f);  // 右侧12像素

        return new EntryUi
        {
            Root = go,
            Background = bg,
            Name = name,
            Status = status,
        };
    }

    /// <summary>
    /// 创建翻页按钮（"<" 或 ">"）
    /// </summary>
    /// <param name="parent">父容器</param>
    /// <param name="name">按钮对象名称</param>
    /// <param name="text">按钮显示的文字（"<" 或 ">"）</param>
    /// <returns>创建的按钮组件</returns>
    private static Button CreatePageButton(Transform parent, string name, string text)
    {
        // 创建按钮容器
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        // 设置按钮尺寸（24x24像素）
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(24f, 24f);

        // 设置按钮背景图像（半透明白色）
        Image img = go.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = new Color(1f, 1f, 1f, 0.15f);  // 白色，15%不透明度

        // 配置按钮组件
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;  // 点击时改变此图像的颜色

        // 创建按钮标签文本（"<" 或 ">"）
        TextMeshProUGUI label = CreateTmpText(go.transform, "Label", text, 18);
        label.alignment = TextAlignmentOptions.Center;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        // 标签铺满整个按钮
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return btn;
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

    #endregion

    #region 工具方法

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

    /// <summary>
    /// 尝试将object转换为JsonElement
    /// </summary>
    /// <param name="payload">输入对象</param>
    /// <param name="element">输出的JsonElement（如果转换失败则为default）</param>
    /// <returns>转换是否成功</returns>
    private static bool TryGetJsonElement(object payload, out JsonElement element)
    {
        if (payload is JsonElement je)
        {
            element = je;
            return true;
        }

        element = default;
        return false;
    }

    /// <summary>
    /// 从JsonElement中提取字符串值，支持多种JSON类型的转换
    /// </summary>
    /// <param name="elem">JSON元素</param>
    /// <param name="property">属性名称</param>
    /// <returns>转换后的字符串，如果属性不存在或无法转换则返回null</returns>
    private static string GetString(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
        {
            return null;
        }

        // 根据JSON值类型进行转换
        return p.ValueKind switch
        {
            JsonValueKind.String => p.GetString(),     // 直接返回字符串
            JsonValueKind.Number => p.GetRawText(),    // 返回数字的文本表示
            JsonValueKind.True => "true",              // 布尔值转为字符串
            JsonValueKind.False => "false",
            _ => null,  // 其他类型返回null
        };
    }

    /// <summary>
    /// 从JsonElement中提取布尔值，支持多种JSON类型的转换
    /// </summary>
    /// <param name="elem">JSON元素</param>
    /// <param name="property">属性名称</param>
    /// <returns>转换后的布尔值，如果属性不存在或无法转换则返回false</returns>
    private static bool GetBool(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
        {
            return false;
        }

        // 根据JSON值类型进行转换
        return p.ValueKind switch
        {
            JsonValueKind.True => true,                    // 直接返回true
            JsonValueKind.False => false,                  // 直接返回false
            JsonValueKind.Number => p.TryGetInt32(out int v) && v != 0,  // 非零数字为true
            JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) && b,  // 尝试解析字符串
            _ => false,  // 其他类型返回false
        };
    }


    /// <summary>
    /// 从 JsonElement 中提取整数值，支持多种 JSON 类型的转换
    /// </summary>
    /// <param name="elem">JSON 元素</param>
    /// <param name="property">属性名称</param>
    /// <param name="defaultValue">缺省值</param>
    /// <returns>转换后的整数值</returns>
    private static int GetInt(JsonElement elem, string property, int defaultValue = 0)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
        {
            return defaultValue;
        }

        return p.ValueKind switch
        {
            JsonValueKind.Number => p.TryGetInt32(out int v) ? v : defaultValue,
            JsonValueKind.String => int.TryParse(p.GetString(), out int v) ? v : defaultValue,
            JsonValueKind.True => 1,
            JsonValueKind.False => 0,
            _ => defaultValue,
        };
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
        /// <summary>根容器GameObject</summary>
        public GameObject Root { get; set; }

        /// <summary>标题文本控件</summary>
        public TextMeshProUGUI Title { get; set; }

        /// <summary>上一页按钮</summary>
        public Button PrevPage { get; set; }

        /// <summary>下一页按钮</summary>
        public Button NextPage { get; set; }

        /// <summary>玩家条目列表（固定大小）</summary>
        public List<EntryUi> Entries { get; set; }
    }

    /// <summary>
    /// 单个玩家条目UI的数据结构
    /// </summary>
    private sealed class EntryUi
    {
        /// <summary>条目容器GameObject</summary>
        public GameObject Root { get; set; }

        /// <summary>条目背景图像</summary>
        public Image Background { get; set; }

        /// <summary>玩家名称文本</summary>
        public TextMeshProUGUI Name { get; set; }

        /// <summary>在线状态文本（"在线"或"离线"）</summary>
        public TextMeshProUGUI Status { get; set; }
    }
}
    #endregion
