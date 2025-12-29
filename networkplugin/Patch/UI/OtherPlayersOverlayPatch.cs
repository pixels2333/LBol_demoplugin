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
/// “其他玩家角色实体更新/渲染”补丁（参考 Together in Spire：CharacterEntityUpdateAndRender.java）
/// <para/>
/// LBoL 是 Unity 渲染管线，通常不直接 Patch “render()”，因此本补丁采用：
/// 1) Patch `GameDirector.Update()` 作为全局每帧入口（类比 AbstractDungeon.update / AbstractRoom.update）
/// 2) 通过 Unity UI 在屏幕上生成“玩家信息框 + 翻页按钮”（类比 RenderInfoBoxes/UpdateInfoBoxes）
/// 3) 通过监听网络事件维护“其他玩家列表”（类比 P2PManager.GetAllPlayers）
/// </summary>
[HarmonyPatch]
public static class OtherPlayersOverlayPatch
{
    private const int PageSize = 10;

    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object _syncLock = new();
    private static readonly Dictionary<string, PlayerSummary> _players = new();

    private static int _page;
    private static OverlayUi _ui;
    private static TMP_FontAsset _defaultFont;

    private static INetworkClient _subscribedClient;
    private static bool _subscribed;

    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    [HarmonyPatch(typeof(GameDirector), "Update")]
    [HarmonyPostfix]
    public static void GameDirector_Update_Postfix()
    {
        try
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
            {
                HideUi();
                return;
            }

            EnsureSubscribed(client);

            // 网络逻辑通常需要每帧轮询；为了让“玩家列表更新/加入事件”能及时驱动 UI，顺便在这里做一次轮询。
            try
            {
                client.PollEvents();
            }
            catch
            {
                // 忽略：有些实现可能要求 Start/Connect 后才能 PollEvents。
            }

            if (!client.IsConnected)
            {
                HideUi();
                return;
            }

            // UiManager 的很多状态/层级是私有成员，这里尽量用公开静态属性 + 反射兜底。
            if (UiManager.Instance == null || UiManager.IsShowingLoading || UiManager.IsBlockingInput)
            {
                HideUi();
                return;
            }

            EnsureUi();
            RefreshUi();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[OtherPlayersOverlayPatch] GameDirector_Update_Postfix 异常: {ex}");
        }
    }

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

    private static void EnsureSubscribed(INetworkClient client)
    {
        if (_subscribed && ReferenceEquals(_subscribedClient, client))
        {
            return;
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= _onGameEventReceived;
                _subscribedClient.OnConnectionStateChanged -= _onConnectionStateChanged;
            }
        }
        catch
        {
            // 忽略：旧客户端可能已被销毁/异常。
        }

        try
        {
            client.OnGameEventReceived += _onGameEventReceived;
            client.OnConnectionStateChanged += _onConnectionStateChanged;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogWarning($"[OtherPlayersOverlayPatch] 订阅网络事件失败: {ex.Message}");
            _subscribed = false;
            _subscribedClient = null;
        }
    }

    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        lock (_syncLock)
        {
            _players.Clear();
            _page = 0;
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        try
        {
            if (!TryGetJsonElement(payload, out JsonElement root))
            {
                return;
            }

            switch (eventType)
            {
                case "PlayerListUpdate":
                    HandlePlayerListUpdate(root);
                    break;
                case "PlayerJoined":
                    HandlePlayerJoined(root);
                    break;
                case "HostChanged":
                    HandleHostChanged(root);
                    break;
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[OtherPlayersOverlayPatch] 处理网络事件失败: {eventType}, {ex.Message}");
        }
    }

    private static void HandlePlayerListUpdate(JsonElement root)
    {
        if (!root.TryGetProperty("Players", out JsonElement playersElem) || playersElem.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var incoming = new Dictionary<string, PlayerSummary>();
        foreach (JsonElement p in playersElem.EnumerateArray())
        {
            string playerId = GetString(p, "PlayerId");
            if (string.IsNullOrWhiteSpace(playerId))
            {
                continue;
            }

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
            _players.Clear();
            foreach (var kv in incoming)
            {
                _players[kv.Key] = kv.Value;
            }

            ClampPage_NoLock();
        }
    }

    private static void HandlePlayerJoined(JsonElement root)
    {
        string playerId = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (_syncLock)
        {
            _players[playerId] = new PlayerSummary
            {
                PlayerId = playerId,
                PlayerName = GetString(root, "PlayerName") ?? playerId,
                IsHost = GetBool(root, "IsHost"),
                IsConnected = true,
                LastUpdateTime = Time.unscaledTime,
            };

            ClampPage_NoLock();
        }
    }

    private static void HandleHostChanged(JsonElement root)
    {
        string newHostId = GetString(root, "NewHostId");
        if (string.IsNullOrWhiteSpace(newHostId))
        {
            return;
        }

        lock (_syncLock)
        {
            foreach (var kv in _players)
            {
                kv.Value.IsHost = kv.Key == newHostId;
            }
        }
    }

    private static void EnsureUi()
    {
        if (_ui != null && _ui.Root != null)
        {
            _ui.Root.SetActive(true);
            return;
        }

        Transform parent = TryGetUiLayerTransform("topLayer") ?? TryGetUiLayerTransform("topmostLayer") ?? UiManager.Instance.transform;
        _defaultFont ??= FindDefaultFont(parent);

        var root = new GameObject("NetworkPlugin_OtherPlayersOverlay");
        root.transform.SetParent(parent, false);

        var rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 1f);
        rootRect.anchoredPosition = new Vector2(-24f, -24f);
        rootRect.sizeDelta = new Vector2(360f, 520f);

        var bg = root.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = new Color(0f, 0f, 0f, 0.35f);
        bg.raycastTarget = false;

        var title = CreateTmpText(root.transform, "Title", "联机玩家（其他玩家信息框）", 20);
        title.alignment = TextAlignmentOptions.TopLeft;
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -10f);
        titleRect.sizeDelta = new Vector2(-20f, 30f);

        var leftButton = CreatePageButton(root.transform, "PrevPage", "<");
        var leftRect = leftButton.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(1f, 1f);
        leftRect.anchorMax = new Vector2(1f, 1f);
        leftRect.pivot = new Vector2(1f, 1f);
        leftRect.anchoredPosition = new Vector2(-66f, -10f);
        leftRect.sizeDelta = new Vector2(24f, 24f);
        leftButton.onClick.AddListener(() =>
        {
            lock (_syncLock)
            {
                _page = Mathf.Max(0, _page - 1);
            }
        });

        var rightButton = CreatePageButton(root.transform, "NextPage", ">");
        var rightRect = rightButton.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(1f, 1f);
        rightRect.anchorMax = new Vector2(1f, 1f);
        rightRect.pivot = new Vector2(1f, 1f);
        rightRect.anchoredPosition = new Vector2(-36f, -10f);
        rightRect.sizeDelta = new Vector2(24f, 24f);
        rightButton.onClick.AddListener(() =>
        {
            lock (_syncLock)
            {
                _page += 1;
                ClampPage_NoLock();
            }
        });

        var entries = new List<EntryUi>(PageSize);
        for (int i = 0; i < PageSize; i++)
        {
            entries.Add(CreateEntry(root.transform, i));
        }

        _ui = new OverlayUi
        {
            Root = root,
            Title = title,
            PrevPage = leftButton,
            NextPage = rightButton,
            Entries = entries,
        };
    }

    private static void RefreshUi()
    {
        List<PlayerSummary> list;
        int page;

        lock (_syncLock)
        {
            list = _players.Values
                .OrderByDescending(p => p.IsHost)
                .ThenByDescending(p => p.IsConnected)
                .ThenBy(p => p.PlayerName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            page = _page;
        }

        if (list.Count == 0)
        {
            HideUi();
            return;
        }

        int pageCount = Mathf.Max(1, Mathf.CeilToInt(list.Count / (float)PageSize));
        page = Mathf.Clamp(page, 0, pageCount - 1);
        lock (_syncLock)
        {
            _page = page;
        }

        int startIndex = page * PageSize;
        int endIndex = Mathf.Min(startIndex + PageSize, list.Count);
        int visibleCount = Mathf.Max(0, endIndex - startIndex);

        _ui.Root.SetActive(true);
        _ui.Title.text = $"联机玩家（第 {page + 1}/{pageCount} 页）";

        _ui.PrevPage.interactable = page > 0;
        _ui.NextPage.interactable = page + 1 < pageCount;

        for (int i = 0; i < _ui.Entries.Count; i++)
        {
            bool visible = i < visibleCount;
            _ui.Entries[i].Root.SetActive(visible);
            if (!visible)
            {
                continue;
            }

            PlayerSummary p = list[startIndex + i];
            string hostTag = p.IsHost ? " [房主]" : "";
            string connTag = p.IsConnected ? "在线" : "离线";

            _ui.Entries[i].Name.text = $"{p.PlayerName}{hostTag}";
            _ui.Entries[i].Status.text = connTag;

            Color baseColor = p.IsConnected ? new Color(0f, 0f, 0f, 0.25f) : new Color(0.2f, 0.0f, 0.0f, 0.25f);
            _ui.Entries[i].Background.color = baseColor;
        }
    }

    private static void HideUi()
    {
        if (_ui?.Root != null)
        {
            _ui.Root.SetActive(false);
        }
    }

    private static void ClampPage_NoLock()
    {
        int count = _players.Count;
        int pageCount = Mathf.Max(1, Mathf.CeilToInt(count / (float)PageSize));
        _page = Mathf.Clamp(_page, 0, pageCount - 1);
    }

    private static EntryUi CreateEntry(Transform parent, int index)
    {
        const float entryHeight = 42f;
        const float topOffset = 48f;
        const float leftPadding = 12f;
        const float rightPadding = 12f;

        var go = new GameObject($"Entry_{index:00}");
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -(topOffset + index * (entryHeight + 6f)));
        rect.sizeDelta = new Vector2(-24f, entryHeight);

        var bg = go.AddComponent<Image>();
        bg.sprite = GetWhiteSprite();
        bg.color = new Color(0f, 0f, 0f, 0.25f);
        bg.raycastTarget = false;

        var name = CreateTmpText(go.transform, "Name", "Player", 18);
        name.alignment = TextAlignmentOptions.Left;
        var nameRect = name.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = new Vector2(leftPadding, 4f);
        nameRect.offsetMax = new Vector2(-110f, -4f);

        var status = CreateTmpText(go.transform, "Status", "在线", 16);
        status.alignment = TextAlignmentOptions.Right;
        var statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 1f);
        statusRect.offsetMin = new Vector2(160f, 4f);
        statusRect.offsetMax = new Vector2(-rightPadding, -4f);

        return new EntryUi
        {
            Root = go,
            Background = bg,
            Name = name,
            Status = status,
        };
    }

    private static Button CreatePageButton(Transform parent, string name, string text)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(24f, 24f);

        var img = go.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = new Color(1f, 1f, 1f, 0.15f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        var label = CreateTmpText(go.transform, "Label", text, 18);
        label.alignment = TextAlignmentOptions.Center;
        var labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return btn;
    }

    private static TextMeshProUGUI CreateTmpText(Transform parent, string name, string text, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        if (_defaultFont != null)
        {
            tmp.font = _defaultFont;
        }
        return tmp;
    }

    private static TMP_FontAsset FindDefaultFont(Transform root)
    {
        try
        {
            var tmp = root.GetComponentInChildren<TextMeshProUGUI>(true);
            return tmp != null ? tmp.font : null;
        }
        catch
        {
            return null;
        }
    }

    private static Transform TryGetUiLayerTransform(string fieldName)
    {
        try
        {
            var rect = Traverse.Create(UiManager.Instance).Field(fieldName).GetValue<RectTransform>();
            return rect != null ? rect.transform : null;
        }
        catch
        {
            return null;
        }
    }

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

    private static string GetString(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
        {
            return null;
        }

        return p.ValueKind switch
        {
            JsonValueKind.String => p.GetString(),
            JsonValueKind.Number => p.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null,
        };
    }

    private static bool GetBool(JsonElement elem, string property)
    {
        if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
        {
            return false;
        }

        return p.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => p.TryGetInt32(out int v) && v != 0,
            JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) && b,
            _ => false,
        };
    }

    private static Sprite _whiteSprite;
    private static Texture2D _whiteTexture;

    private static Sprite GetWhiteSprite()
    {
        if (_whiteSprite != null)
        {
            return _whiteSprite;
        }

        _whiteTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        _whiteTexture.SetPixel(0, 0, Color.white);
        _whiteTexture.Apply(false, true);
        _whiteSprite = Sprite.Create(_whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _whiteSprite;
    }

    private sealed class PlayerSummary
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public bool IsHost { get; set; }
        public bool IsConnected { get; set; }
        public float LastUpdateTime { get; set; }
    }

    private sealed class OverlayUi
    {
        public GameObject Root { get; set; }
        public TextMeshProUGUI Title { get; set; }
        public Button PrevPage { get; set; }
        public Button NextPage { get; set; }
        public List<EntryUi> Entries { get; set; }
    }

    private sealed class EntryUi
    {
        public GameObject Root { get; set; }
        public Image Background { get; set; }
        public TextMeshProUGUI Name { get; set; }
        public TextMeshProUGUI Status { get; set; }
    }
}
