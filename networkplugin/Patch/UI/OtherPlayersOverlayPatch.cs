using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using LBoL.Presentation.UI;
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
            // 获取网络客户端实例
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
            {
                HideUi();
                return;
            }

            // 确保已订阅网络事件
            EnsureSubscribed(client);

            // 每帧轮询网络事件，让"玩家列表更新/加入事件"能及时驱动 UI
            try
            {
                client.PollEvents();
            }
            catch
            {
                // 忽略：有些实现可能要求 Start/Connect 后才能 PollEvents
            }

            // 如果未连接，隐藏UI
            if (!client.IsConnected)
            {
                HideUi();
                return;
            }

            // 检查游戏UI状态，避免在加载或阻塞输入时显示overlay
            // UiManager 的很多状态/层级是私有成员，这里尽量用公开静态属性 + 反射兜底
            if (UiManager.Instance == null || UiManager.IsShowingLoading || UiManager.IsBlockingInput)
            {
                HideUi();
                return;
            }

            // 创建或显示UI
            EnsureUi();
            // 刷新UI内容
            RefreshUi();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[OtherPlayersOverlayPatch] GameDirector_Update_Postfix 异常: {ex}");
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
                case "PlayerListUpdate":
                    // 玩家列表更新事件：完整覆盖现有玩家列表
                    HandlePlayerListUpdate(root);
                    break;
                case "PlayerJoined":
                    // 新玩家加入事件：添加新玩家
                    HandlePlayerJoined(root);
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
        // 验证JSON数据包含Players属性且为数组
        if (!root.TryGetProperty("Players", out JsonElement playersElem) || playersElem.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        // 临时字典用于存储新的玩家数据
        var incoming = new Dictionary<string, PlayerSummary>();
        foreach (JsonElement p in playersElem.EnumerateArray())
        {
            // 获取PlayerId，如果为空则跳过此条目
            string playerId = GetString(p, "PlayerId");
            if (string.IsNullOrWhiteSpace(playerId))
            {
                continue;
            }

            // 创建并添加玩家摘要信息
            incoming[playerId] = new PlayerSummary
            {
                PlayerId = playerId,
                PlayerName = GetString(p, "PlayerName") ?? playerId,
                IsHost = GetBool(p, "IsHost"),
                IsConnected = GetBool(p, "IsConnected"),
                LastUpdateTime = Time.unscaledTime,
            };
        }

        lock (_syncLock)
        {
            // 完全替换现有玩家列表
            _players.Clear();
            foreach (var kv in incoming)
            {
                _players[kv.Key] = kv.Value;
            }

            // 调整页码以确保不超过范围
            ClampPage_NoLock();
        }
    }

    /// <summary>
    /// 处理新玩家加入事件
    /// </summary>
    /// <param name="root">JSON数据根元素，应包含 "PlayerId"、"PlayerName"、"IsHost"</param>
    private static void HandlePlayerJoined(JsonElement root)
    {
        // 提取玩家ID，如果为空则忽略此事件
        string playerId = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (_syncLock)
        {
            // 向玩家列表添加新玩家
            _players[playerId] = new PlayerSummary
            {
                PlayerId = playerId,
                PlayerName = GetString(root, "PlayerName") ?? playerId,
                IsHost = GetBool(root, "IsHost"),
                IsConnected = true,  // 新加入的玩家默认为在线
                LastUpdateTime = Time.unscaledTime,
            };

            // 调整页码以确保不超过范围
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
            foreach (var kv in _players)
            {
                kv.Value.IsHost = kv.Key == newHostId;
            }
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
        var root = new GameObject("NetworkPlugin_OtherPlayersOverlay");
        root.transform.SetParent(parent, false);

        // 设置根容器的RectTransform（位置在右下角，距离屏幕边缘24像素）
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);  // 锚点：右上角
        rootRect.anchorMax = new Vector2(1f, 1f);  // 锚点：右上角
        rootRect.pivot = new Vector2(1f, 1f);      // 中心点：右上角
        rootRect.anchoredPosition = new Vector2(-24f, -24f);  // 距离锚点偏移
        rootRect.sizeDelta = new Vector2(360f, 520f);  // 宽360，高520

        // 设置背景图像（半透明黑色）
        var bg = root.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = new Color(0f, 0f, 0f, 0.35f);  // 黑色，35%不透明度
        bg.raycastTarget = false;  // 不阻挡射线检测

        // 创建标题文本（"联机玩家（其他玩家信息框）"）
        var title = CreateTmpText(root.transform, "Title", "联机玩家（其他玩家信息框）", 20);
        title.alignment = TextAlignmentOptions.TopLeft;
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);  // 左上角
        titleRect.anchorMax = new Vector2(1f, 1f);  // 右上角
        titleRect.pivot = new Vector2(0.5f, 1f);   // 中上
        titleRect.anchoredPosition = new Vector2(0f, -10f);  // 距上边10像素
        titleRect.sizeDelta = new Vector2(-20f, 30f);  // 留出左右各10像素的空白

        // 创建上一页按钮（"<"）
        var leftButton = CreatePageButton(root.transform, "PrevPage", "<");
        var leftRect = leftButton.GetComponent<RectTransform>();
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
        var rightButton = CreatePageButton(root.transform, "NextPage", ">");
        var rightRect = rightButton.GetComponent<RectTransform>();
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
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);      // 左上角
        rect.anchorMax = new Vector2(1f, 1f);      // 右上角
        rect.pivot = new Vector2(0.5f, 1f);        // 中上
        // 垂直位置：第一个条目距标题48像素，之后每个条目递进 48像素
        rect.anchoredPosition = new Vector2(0f, -(topOffset + index * (entryHeight + 6f)));
        rect.sizeDelta = new Vector2(-24f, entryHeight);  // 宽度留出左右各12像素，高度42

        // 设置条目背景
        var bg = go.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = new Color(0f, 0f, 0f, 0.25f);  // 浅灰色背景
        bg.raycastTarget = false;  // 不阻挡射线检测

        // 创建玩家名称文本（显示在左侧）
        var name = CreateTmpText(go.transform, "Name", "Player", 18);
        name.alignment = TextAlignmentOptions.Left;
        var nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = new Vector2(leftPadding, 4f);       // 左侧12像素，上下各4像素
        nameRect.offsetMax = new Vector2(-110f, -4f);            // 右侧留110像素给状态文本

        // 创建在线状态文本（显示在右侧）
        var status = CreateTmpText(go.transform, "Status", "在线", 16);
        status.alignment = TextAlignmentOptions.Right;
        var statusRect = status.GetComponent<RectTransform>();
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
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(24f, 24f);

        // 设置按钮背景图像（半透明白色）
        var img = go.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = new Color(1f, 1f, 1f, 0.15f);  // 白色，15%不透明度

        // 配置按钮组件
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;  // 点击时改变此图像的颜色

        // 创建按钮标签文本（"<" 或 ">"）
        var label = CreateTmpText(go.transform, "Label", text, 18);
        label.alignment = TextAlignmentOptions.Center;
        var labelRect = label.GetComponent<RectTransform>();
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
        var tmp = go.AddComponent<TextMeshProUGUI>();
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
            var tmp = root.GetComponentInChildren<TextMeshProUGUI>(true);
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
            var rect = Traverse.Create(UiManager.Instance).Field(fieldName).GetValue<RectTransform>();
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
