using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.NetworkPlayer;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.Map;

/// <summary>
/// 地图节点右键标记同步与渲染（参照 Together in Spire: MapNodePatches）。
/// - 右键地图节点：切换“标记”状态，并通过 GameEvent 广播给其他玩家。
/// - 打开地图/刷新节点状态：在每个 <see cref="MapNodeWidget"/> 上叠加一个标记背景。
/// </summary>
[HarmonyPatch]
public static class MapNodeMarkSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object SyncLock = new();
    private static readonly Dictionary<NodeKey, bool> Marks = new();

    private static MapNodeWidget _hovered;
    private static Sprite _markSprite;
    private static readonly Dictionary<MapNodeWidget, Image> MarkImages = new();

    private static Sprite _dotSprite;
    private static readonly Dictionary<MapNodeWidget, DotUi> DotUis = new();

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> OnGameEventReceivedHandler = OnGameEventReceived;
    private static readonly Action<bool> OnConnectionStateChangedHandler = OnConnectionStateChanged;

    private sealed class DotUi
    {
        public RectTransform Root;
        public readonly List<Image> Dots = new();
    }

    private readonly struct NodeKey : IEquatable<NodeKey>
    {
        public NodeKey(int act, int x, int y)
        {
            Act = act;
            X = x;
            Y = y;
        }

        public int Act { get; }
        public int X { get; }
        public int Y { get; }

        public bool Equals(NodeKey other) => Act == other.Act && X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is NodeKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Act, X, Y);
        public override string ToString() => $"Act={Act},X={X},Y={Y}";
    }

    private readonly struct NodePosKey : IEquatable<NodePosKey>
    {
        public NodePosKey(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public bool Equals(NodePosKey other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is NodePosKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    private static INetworkClient TryGetClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
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

    private static void EnsureSubscribed(INetworkClient client)
    {
        lock (SyncLock)
        {
            if (_subscribed && ReferenceEquals(_subscribedClient, client))
            {
                return;
            }
        }

        try
        {
            if (_subscribedClient != null)
            {
                _subscribedClient.OnGameEventReceived -= OnGameEventReceivedHandler;
                _subscribedClient.OnConnectionStateChanged -= OnConnectionStateChangedHandler;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            client.OnGameEventReceived += OnGameEventReceivedHandler;
            client.OnConnectionStateChanged += OnConnectionStateChangedHandler;
            lock (SyncLock)
            {
                _subscribedClient = client;
                _subscribed = true;
            }
        }
        catch
        {
            lock (SyncLock)
            {
                _subscribedClient = null;
                _subscribed = false;
            }
        }
    }

    private static void OnConnectionStateChanged(bool connected)
    {
        if (connected)
        {
            return;
        }

        lock (SyncLock)
        {
            Marks.Clear();
        }

        try
        {
            foreach (DotUi ui in DotUis.Values)
            {
                if (ui?.Root != null)
                {
                    ui.Root.gameObject.SetActive(false);
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!string.Equals(eventType, NetworkMessageTypes.OnMapNodeMarkChanged, StringComparison.Ordinal))
        {
            return;
        }

        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        int act = GetInt(root, "Act", -1);
        int x = GetInt(root, "X", -1);
        int y = GetInt(root, "Y", -1);
        bool marked = GetBool(root, "Marked");

        if (act < 0 || x < 0 || y < 0)
        {
            return;
        }

        lock (SyncLock)
        {
            Marks[new NodeKey(act, x, y)] = marked;
        }

        try
        {
            MapPanel mapPanel = UiManager.GetPanel<MapPanel>();
            if (mapPanel != null && mapPanel.IsVisible)
            {
                RefreshMarks(mapPanel);
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(MapNodeWidget), nameof(MapNodeWidget.OnPointerEnter))]
    [HarmonyPostfix]
    public static void MapNodeWidget_OnPointerEnter_Postfix(MapNodeWidget __instance)
    {
        _hovered = __instance;
        UiManager.HoveringRightClickInteractionElements = true;
    }

    [HarmonyPatch(typeof(MapNodeWidget), nameof(MapNodeWidget.OnPointerExit))]
    [HarmonyPostfix]
    public static void MapNodeWidget_OnPointerExit_Postfix(MapNodeWidget __instance)
    {
        if (ReferenceEquals(_hovered, __instance))
        {
            _hovered = null;
        }

        UiManager.HoveringRightClickInteractionElements = false;
    }

    [HarmonyPatch(typeof(UiManager), "RightClicked")]
    [HarmonyPrefix]
    public static void UiManager_RightClicked_Prefix()
    {
        try
        {
            if (_hovered == null || _hovered.MapNode == null)
            {
                return;
            }

            MapPanel mapPanel = UiManager.GetPanel<MapPanel>();
            if (mapPanel == null || !mapPanel.IsVisible)
            {
                return;
            }

            ToggleLocalMarkAndBroadcast(_hovered);
            RefreshMarks(mapPanel);
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(MapPanel), "UpdateMapNodesStatus")]
    [HarmonyPostfix]
    public static void MapPanel_UpdateMapNodesStatus_Postfix(MapPanel __instance)
    {
        try
        {
            INetworkClient client = TryGetClient();
            if (client != null)
            {
                EnsureSubscribed(client);
            }

            RefreshMarks(__instance);
        }
        catch
        {
            // ignored
        }
    }

    private static void ToggleLocalMarkAndBroadcast(MapNodeWidget widget)
    {
        MapNode node = widget?.MapNode;
        if (node == null)
        {
            return;
        }

        NodeKey key = new(node.Act, node.X, node.Y);
        bool next;
        lock (SyncLock)
        {
            Marks.TryGetValue(key, out bool cur);
            next = !cur;
            Marks[key] = next;
        }

        INetworkClient client = TryGetClient();
        if (client?.IsConnected != true)
        {
            return;
        }

        try
        {
            client.SendGameEventData(
                NetworkMessageTypes.OnMapNodeMarkChanged,
                new
                {
                    Act = key.Act,
                    X = key.X,
                    Y = key.Y,
                    Marked = next,
                    Timestamp = DateTime.Now.Ticks
                }
            );
        }
        catch
        {
            // ignored
        }
    }

    private static void RefreshMarks(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return;
        }

        Dictionary<NodePosKey, List<INetworkPlayer>> playersByPos = null;
        try
        {
            INetworkManager manager = TryGetNetworkManager();
            if (manager?.IsConnected == true)
            {
                INetworkPlayer self = manager.GetSelf();
                foreach (INetworkPlayer p in manager.GetAllPlayers())
                {
                    if (p == null || ReferenceEquals(p, self))
                    {
                        continue;
                    }

                    int px = p.location_X;
                    int py = p.location_Y;
                    if (px < 0 || py < 0)
                    {
                        continue;
                    }

                    playersByPos ??= new Dictionary<NodePosKey, List<INetworkPlayer>>();
                    NodePosKey keyPos = new(px, py);
                    if (!playersByPos.TryGetValue(keyPos, out List<INetworkPlayer> list))
                    {
                        list = new List<INetworkPlayer>();
                        playersByPos[keyPos] = list;
                    }

                    list.Add(p);
                }
            }
        }
        catch
        {
            // ignored
        }

        MapNodeWidget[,] widgets;
        try
        {
            widgets = Traverse.Create(mapPanel).Field("_mapNodeWidgets").GetValue<MapNodeWidget[,]>();
        }
        catch
        {
            return;
        }

        if (widgets == null)
        {
            return;
        }

        int maxX = widgets.GetUpperBound(0);
        int maxY = widgets.GetUpperBound(1);
        for (int x = widgets.GetLowerBound(0); x <= maxX; x++)
        {
            for (int y = widgets.GetLowerBound(1); y <= maxY; y++)
            {
                MapNodeWidget w = widgets[x, y];
                if (w == null || w.MapNode == null)
                {
                    continue;
                }

                NodeKey key = new(w.MapNode.Act, w.MapNode.X, w.MapNode.Y);
                bool marked;
                lock (SyncLock)
                {
                    marked = Marks.TryGetValue(key, out bool m) && m;
                }

                Image img = EnsureMarkImage(w);
                if (img == null)
                {
                    continue;
                }

                img.enabled = marked;
                if (marked)
                {
                    img.color = new Color(1f, 0.85f, 0.2f, 0.35f);
                }

                List<INetworkPlayer> here = null;
                if (playersByPos != null)
                {
                    playersByPos.TryGetValue(new NodePosKey(w.MapNode.X, w.MapNode.Y), out here);
                }

                UpdatePlayerDots(w, here);
            }
        }
    }

    private static Image EnsureMarkImage(MapNodeWidget widget)
    {
        if (widget == null)
        {
            return null;
        }

        if (MarkImages.TryGetValue(widget, out Image existing) && existing != null)
        {
            return existing;
        }

        if (_markSprite == null)
        {
            _markSprite = CreateWhiteSprite();
        }

        if (_markSprite == null)
        {
            return null;
        }

        GameObject go = new("NetworkPlugin_NodeMark");
        go.transform.SetParent(widget.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(220f, 220f);

        Image img = go.AddComponent<Image>();
        img.sprite = _markSprite;
        img.raycastTarget = false;
        img.enabled = false;

        MarkImages[widget] = img;
        return img;
    }

    private static void UpdatePlayerDots(MapNodeWidget widget, List<INetworkPlayer> playersHere)
    {
        int count = playersHere?.Count ?? 0;
        if (count <= 0)
        {
            if (DotUis.TryGetValue(widget, out DotUi existing) && existing?.Root != null)
            {
                existing.Root.gameObject.SetActive(false);
            }
            return;
        }

        DotUi ui = EnsureDotUi(widget);
        if (ui?.Root == null)
        {
            return;
        }

        ui.Root.gameObject.SetActive(true);

        if (_dotSprite == null)
        {
            _dotSprite = CreateDotSprite();
        }

        int displayCount = Mathf.Min(4, count);
        while (ui.Dots.Count < displayCount)
        {
            GameObject go = new("Dot");
            go.transform.SetParent(ui.Root, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(18f, 18f);

            Image img = go.AddComponent<Image>();
            img.sprite = _dotSprite;
            img.raycastTarget = false;
            ui.Dots.Add(img);
        }

        for (int i = 0; i < ui.Dots.Count; i++)
        {
            bool active = i < displayCount;
            Image img = ui.Dots[i];
            if (img != null)
            {
                img.enabled = active;
            }
        }

        ApplyDotLayout(ui, displayCount);

        for (int i = 0; i < displayCount; i++)
        {
            INetworkPlayer p = playersHere[i];
            if (p == null)
            {
                continue;
            }

            Image img = ui.Dots[i];
            if (img != null)
            {
                img.color = GetPlayerDotColor(p.userName);
            }
        }
    }

    private static DotUi EnsureDotUi(MapNodeWidget widget)
    {
        if (widget == null)
        {
            return null;
        }

        if (DotUis.TryGetValue(widget, out DotUi existing) && existing?.Root != null)
        {
            return existing;
        }

        GameObject go = new("NetworkPlugin_PlayerDots");
        go.transform.SetParent(widget.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(90f, 90f);

        var ui = new DotUi { Root = rt };
        DotUis[widget] = ui;
        go.SetActive(false);
        return ui;
    }

    private static void ApplyDotLayout(DotUi ui, int count)
    {
        if (ui?.Root == null)
        {
            return;
        }

        Vector2[] offsets = count switch
        {
            1 => new[] { Vector2.zero },
            2 => new[] { new Vector2(-14f, 0f), new Vector2(14f, 0f) },
            3 => new[] { new Vector2(0f, 14f), new Vector2(-12f, -10f), new Vector2(12f, -10f) },
            _ => new[] { new Vector2(-12f, 12f), new Vector2(12f, 12f), new Vector2(-12f, -12f), new Vector2(12f, -12f) },
        };

        for (int i = 0; i < count && i < ui.Dots.Count; i++)
        {
            Image img = ui.Dots[i];
            if (img == null)
            {
                continue;
            }

            RectTransform rt = img.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = offsets[i];
            }
        }
    }

    private static Color GetPlayerDotColor(string seed)
    {
        unchecked
        {
            int hash = 17;
            foreach (char c in seed ?? string.Empty)
            {
                hash = hash * 31 + c;
            }

            float h = Mathf.Abs(hash % 360) / 360f;
            Color c2 = Color.HSVToRGB(h, 0.7f, 1f);
            c2.a = 0.95f;
            return c2;
        }
    }

    private static Sprite CreateDotSprite()
    {
        try
        {
            const int size = 32;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float r = (size - 1) * 0.5f;
            float cx = r;
            float cy = r;
            float rr = r * r;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx;
                    float dy = y - cy;
                    float d2 = dx * dx + dy * dy;
                    float a = d2 <= rr ? 1f : 0f;
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }

            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
        catch
        {
            return null;
        }
    }

    private static Sprite CreateWhiteSprite()
    {
        try
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        catch
        {
            return null;
        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
    {
        try
        {
            if (payload is JsonElement je)
            {
                root = je;
                return true;
            }

            if (payload is string s)
            {
                root = JsonDocument.Parse(s).RootElement;
                return true;
            }
        }
        catch
        {
            // ignored
        }

        root = default;
        return false;
    }

    private static int GetInt(JsonElement elem, string property, int defaultValue = 0)
    {
        try
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
                _ => defaultValue
            };
        }
        catch
        {
            return defaultValue;
        }
    }

    private static bool GetBool(JsonElement elem, string property)
    {
        try
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
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }
}
