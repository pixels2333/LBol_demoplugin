using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace NetworkPlugin.Patch.Map;

/// <summary>
/// 地图节点投票同步与渲染。
/// - 玩家右键地图节点：提交当前轮投票。
/// - 房主在“全员投票完成”后裁决节点并广播结果。
/// - 地图刷新时：显示投票候选（黄色）与裁决结果（绿色）以及投票玩家圆点。
/// </summary>
public static class MapNodeMarkSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object SyncLock = new();
    private static readonly Dictionary<string, NodeKey> PlayerVotes = new(StringComparer.Ordinal);
    private static VoteSource? CurrentVoteSource;
    private static NodeKey? ResolvedNode;
    private static int _resolutionNonce;

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

    private readonly struct VoteSource : IEquatable<VoteSource>
    {
        public VoteSource(int act, int x, int y)
        {
            Act = act;
            X = x;
            Y = y;
        }

        public int Act { get; }
        public int X { get; }
        public int Y { get; }

        public bool Equals(VoteSource other) => Act == other.Act && X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is VoteSource other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Act, X, Y);
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

        public bool Matches(MapNode node)
            => node != null && Act == node.Act && X == node.X && Y == node.Y;

        public bool Equals(NodeKey other) => Act == other.Act && X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is NodeKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Act, X, Y);
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
        => ServiceProvider?.GetService<INetworkClient>();

    private static INetworkManager TryGetNetworkManager()
        => ServiceProvider?.GetService<INetworkManager>();

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
            PlayerVotes.Clear();
            CurrentVoteSource = null;
            ResolvedNode = null;
            _resolutionNonce = 0;
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
        if (string.Equals(eventType, NetworkMessageTypes.OnMapNodeVoteCast, StringComparison.Ordinal))
        {
            HandleVoteCast(payload);
            return;
        }

        if (string.Equals(eventType, NetworkMessageTypes.OnMapNodeVoteResult, StringComparison.Ordinal))
        {
            HandleVoteResult(payload);
        }
    }

    private static void HandleVoteCast(object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        string playerId = GetString(root, "PlayerId");
        int sourceAct = GetInt(root, "SourceAct", -1);
        int sourceX = GetInt(root, "SourceX", -1);
        int sourceY = GetInt(root, "SourceY", -1);
        int act = GetInt(root, "Act", -1);
        int x = GetInt(root, "X", -1);
        int y = GetInt(root, "Y", -1);

        if (string.IsNullOrWhiteSpace(playerId) || sourceAct < 0 || sourceX < 0 || sourceY < 0 || act < 0 || x < 0 || y < 0)
        {
            return;
        }

        VoteSource source = new(sourceAct, sourceX, sourceY);
        NodeKey vote = new(act, x, y);

        lock (SyncLock)
        {
            if (CurrentVoteSource.HasValue && !CurrentVoteSource.Value.Equals(source))
            {
                return;
            }

            CurrentVoteSource = source;
            PlayerVotes[playerId] = vote;
            ResolvedNode = null;
        }

        MapPanel mapPanel = TryGetMapPanel();
        TryResolveVotesAndBroadcast(mapPanel);

        if (mapPanel?.IsVisible == true)
        {
            RefreshMarks(mapPanel);
        }
    }

    private static void HandleVoteResult(object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        int sourceAct = GetInt(root, "SourceAct", -1);
        int sourceX = GetInt(root, "SourceX", -1);
        int sourceY = GetInt(root, "SourceY", -1);
        int act = GetInt(root, "Act", -1);
        int x = GetInt(root, "X", -1);
        int y = GetInt(root, "Y", -1);

        if (sourceAct < 0 || sourceX < 0 || sourceY < 0 || act < 0 || x < 0 || y < 0)
        {
            return;
        }

        VoteSource source = new(sourceAct, sourceX, sourceY);
        NodeKey resolved = new(act, x, y);

        lock (SyncLock)
        {
            CurrentVoteSource = source;
            PlayerVotes.Clear();
            ResolvedNode = resolved;
        }

        MapPanel mapPanel = TryGetMapPanel();
        if (mapPanel?.IsVisible == true)
        {
            RefreshMarks(mapPanel);
            TryApplyResolvedNode(mapPanel, resolved);
        }
    }

    private static void TryResolveVotesAndBroadcast(MapPanel mapPanel)
    {
        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        HashSet<string> expectedVoters = GetExpectedVoterIds();
        if (expectedVoters.Count <= 0)
        {
            return;
        }

        VoteSource source;
        List<NodeKey> candidates = new();
        NodeKey resolved;
        int nonce;

        lock (SyncLock)
        {
            if (!CurrentVoteSource.HasValue)
            {
                return;
            }

            source = CurrentVoteSource.Value;
            foreach (string playerId in expectedVoters)
            {
                if (!PlayerVotes.TryGetValue(playerId, out NodeKey vote))
                {
                    return;
                }

                candidates.Add(vote);
            }

            if (candidates.Count <= 0)
            {
                return;
            }

            _resolutionNonce++;
            nonce = _resolutionNonce;
            resolved = PickWinningVote(source, candidates, nonce);
            PlayerVotes.Clear();
            ResolvedNode = resolved;
        }

        INetworkClient client = TryGetClient();
        if (client?.IsConnected == true)
        {
            try
            {
                client.SendGameEventData(
                    NetworkMessageTypes.OnMapNodeVoteResult,
                    new
                    {
                        SourceAct = source.Act,
                        SourceX = source.X,
                        SourceY = source.Y,
                        Act = resolved.Act,
                        X = resolved.X,
                        Y = resolved.Y,
                        Nonce = nonce,
                        CandidateCount = candidates.Count,
                        Timestamp = DateTime.Now.Ticks
                    }
                );
            }
            catch
            {
                // ignored
            }
        }

        if (mapPanel?.IsVisible == true)
        {
            RefreshMarks(mapPanel);
            TryApplyResolvedNode(mapPanel, resolved);
        }
    }

    private static NodeKey PickWinningVote(VoteSource source, List<NodeKey> votes, int nonce)
    {
        if (votes == null || votes.Count <= 0)
        {
            return default;
        }

        unchecked
        {
            int seed = 17;
            seed = seed * 31 + source.Act;
            seed = seed * 31 + source.X;
            seed = seed * 31 + source.Y;
            seed = seed * 31 + nonce;
            for (int i = 0; i < votes.Count; i++)
            {
                seed = seed * 31 + votes[i].Act;
                seed = seed * 31 + votes[i].X;
                seed = seed * 31 + votes[i].Y;
            }

            int normalized = seed == int.MinValue ? 0 : Math.Abs(seed);
            System.Random rng = new(normalized);
            return votes[rng.Next(votes.Count)];
        }
    }

    private static HashSet<string> GetExpectedVoterIds()
    {
        HashSet<string> ids;
        try
        {
            ids = NetworkIdentityTracker.GetPlayerIdsSnapshot();
        }
        catch
        {
            ids = new HashSet<string>(StringComparer.Ordinal);
        }

        if (ids == null)
        {
            ids = new HashSet<string>(StringComparer.Ordinal);
        }

        List<string> emptyKeys = null;
        foreach (string id in ids)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                (emptyKeys ??= new List<string>()).Add(id);
            }
        }

        if (emptyKeys != null)
        {
            for (int i = 0; i < emptyKeys.Count; i++)
            {
                ids.Remove(emptyKeys[i]);
            }
        }

        string selfId = GetSelfPlayerId();
        if (!string.IsNullOrWhiteSpace(selfId))
        {
            ids.Add(selfId);
        }

        return ids;
    }

    private static string GetSelfPlayerId()
    {
        string id = null;
        try
        {
            id = NetworkIdentityTracker.GetSelfPlayerId();
        }
        catch
        {
            // ignored
        }

        if (!string.IsNullOrWhiteSpace(id))
        {
            return id;
        }

        try
        {
            INetworkManager manager = TryGetNetworkManager();
            var self = manager?.GetSelf();
            if (!string.IsNullOrWhiteSpace(self?.playerId))
            {
                return self.playerId;
            }

            if (!string.IsNullOrWhiteSpace(self?.userName))
            {
                return self.userName;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static MapPanel TryGetMapPanel()
    {
        try
        {
            return UiManager.GetPanel<MapPanel>();
        }
        catch
        {
            return null;
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

            MapPanel mapPanel = TryGetMapPanel();
            if (mapPanel == null || !mapPanel.IsVisible)
            {
                return;
            }

            SubmitLocalVoteAndBroadcast(_hovered, mapPanel);
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
                NetworkIdentityTracker.EnsureSubscribed(client);
            }

            UpdateVoteSource(__instance);
            RefreshMarks(__instance);
        }
        catch
        {
            // ignored
        }
    }

    private static void UpdateVoteSource(MapPanel mapPanel)
    {
        if (!TryGetCurrentVoteSource(mapPanel, out VoteSource source))
        {
            return;
        }

        lock (SyncLock)
        {
            if (CurrentVoteSource.HasValue && CurrentVoteSource.Value.Equals(source))
            {
                return;
            }

            CurrentVoteSource = source;
            PlayerVotes.Clear();
            ResolvedNode = null;
        }
    }

    private static bool TryGetCurrentVoteSource(MapPanel mapPanel, out VoteSource source)
    {
        source = default;
        if (mapPanel == null)
        {
            return false;
        }

        try
        {
            GameMap gameMap = Traverse.Create(mapPanel).Field("_map").GetValue<GameMap>();
            MapNode visitingNode = gameMap?.VisitingNode;
            if (visitingNode == null)
            {
                return false;
            }

            source = new VoteSource(visitingNode.Act, visitingNode.X, visitingNode.Y);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void SubmitLocalVoteAndBroadcast(MapNodeWidget widget, MapPanel mapPanel)
    {
        MapNode node = widget?.MapNode;
        if (node == null)
        {
            return;
        }

        if (!TryGetCurrentVoteSource(mapPanel, out VoteSource source))
        {
            return;
        }

        string playerId = GetSelfPlayerId();
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        NodeKey vote = new(node.Act, node.X, node.Y);
        lock (SyncLock)
        {
            if (!CurrentVoteSource.HasValue || !CurrentVoteSource.Value.Equals(source))
            {
                CurrentVoteSource = source;
                PlayerVotes.Clear();
                ResolvedNode = null;
            }

            PlayerVotes[playerId] = vote;
            ResolvedNode = null;
        }

        INetworkClient client = TryGetClient();
        if (client?.IsConnected == true)
        {
            try
            {
                client.SendGameEventData(
                    NetworkMessageTypes.OnMapNodeVoteCast,
                    new
                    {
                        PlayerId = playerId,
                        SourceAct = source.Act,
                        SourceX = source.X,
                        SourceY = source.Y,
                        Act = vote.Act,
                        X = vote.X,
                        Y = vote.Y,
                        Timestamp = DateTime.Now.Ticks
                    }
                );
            }
            catch
            {
                // ignored
            }
        }

        TryResolveVotesAndBroadcast(mapPanel);
    }

    private static void TryApplyResolvedNode(MapPanel mapPanel, NodeKey resolved)
    {
        if (mapPanel == null)
        {
            return;
        }

        try
        {
            GameMap gameMap = Traverse.Create(mapPanel).Field("_map").GetValue<GameMap>();
            MapNodeWidget[,] widgets = Traverse.Create(mapPanel).Field("_mapNodeWidgets").GetValue<MapNodeWidget[,]>();
            if (gameMap == null || widgets == null)
            {
                return;
            }

            if (resolved.Matches(gameMap.VisitingNode))
            {
                return;
            }

            MapNodeWidget target = FindWidgetByNode(widgets, resolved);
            if (target?.MapNode == null)
            {
                return;
            }

            gameMap.EnterNode(target.MapNode, freeMove: false, forced: false);
        }
        catch
        {
            // ignored
        }
    }

    private static MapNodeWidget FindWidgetByNode(MapNodeWidget[,] widgets, NodeKey target)
    {
        if (widgets == null)
        {
            return null;
        }

        int maxX = widgets.GetUpperBound(0);
        int maxY = widgets.GetUpperBound(1);
        for (int x = widgets.GetLowerBound(0); x <= maxX; x++)
        {
            for (int y = widgets.GetLowerBound(1); y <= maxY; y++)
            {
                MapNodeWidget widget = widgets[x, y];
                if (widget?.MapNode == null)
                {
                    continue;
                }

                if (target.Matches(widget.MapNode))
                {
                    return widget;
                }
            }
        }

        return null;
    }

    private static void RefreshMarks(MapPanel mapPanel)
    {
        if (mapPanel == null)
        {
            return;
        }

        Dictionary<NodePosKey, List<string>> votersByPos = new();
        NodeKey? resolvedNode;

        lock (SyncLock)
        {
            foreach (KeyValuePair<string, NodeKey> pair in PlayerVotes)
            {
                NodePosKey keyPos = new(pair.Value.X, pair.Value.Y);
                if (!votersByPos.TryGetValue(keyPos, out List<string> list))
                {
                    list = new List<string>();
                    votersByPos[keyPos] = list;
                }

                list.Add(pair.Key);
            }

            resolvedNode = ResolvedNode;
        }

        Dictionary<NodePosKey, List<string>> displayedPlayerIdsByPos = BuildDisplayedPlayerIdsByPos(mapPanel, votersByPos);

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
                MapNodeWidget widget = widgets[x, y];
                if (widget?.MapNode == null)
                {
                    continue;
                }

                NodeKey key = new(widget.MapNode.Act, widget.MapNode.X, widget.MapNode.Y);
                bool resolved = resolvedNode.HasValue && resolvedNode.Value.Equals(key);
                votersByPos.TryGetValue(new NodePosKey(widget.MapNode.X, widget.MapNode.Y), out List<string> voters);
                bool hasVotes = voters != null && voters.Count > 0;
                displayedPlayerIdsByPos.TryGetValue(new NodePosKey(widget.MapNode.X, widget.MapNode.Y), out List<string> displayPlayerIds);

                Image img = EnsureMarkImage(widget);
                if (img == null)
                {
                    continue;
                }

                img.enabled = resolved || hasVotes;
                if (resolved)
                {
                    img.color = new Color(0.2f, 1f, 0.35f, 0.42f);
                }
                else if (hasVotes)
                {
                    float alpha = Mathf.Clamp(0.18f + 0.06f * voters.Count, 0.18f, 0.45f);
                    img.color = new Color(1f, 0.85f, 0.2f, alpha);
                }

                UpdatePlayerDots(widget, displayPlayerIds);
            }
        }
    }

    private static Dictionary<NodePosKey, List<string>> BuildDisplayedPlayerIdsByPos(MapPanel mapPanel, Dictionary<NodePosKey, List<string>> votersByPos)
    {
        Dictionary<NodePosKey, List<string>> displayed = new();

        if (votersByPos != null)
        {
            foreach (KeyValuePair<NodePosKey, List<string>> pair in votersByPos)
            {
                if (pair.Value == null || pair.Value.Count <= 0)
                {
                    continue;
                }

                List<string> ids = GetOrCreatePlayerIdList(displayed, pair.Key);
                AppendDistinct(ids, pair.Value);
            }
        }

        foreach (var player in OtherPlayersOverlayPatch.SnapshotPlayersDetailed())
        {
            if (string.IsNullOrWhiteSpace(player.PlayerId) || !player.IsConnected)
            {
                continue;
            }

            if (!IsPlayerVisibleOnCurrentMap(mapPanel, player.Stage, player.LocationX, player.LocationY))
            {
                continue;
            }

            NodePosKey pos = new(player.LocationX, player.LocationY);
            List<string> ids = GetOrCreatePlayerIdList(displayed, pos);
            AppendDistinct(ids, player.PlayerId);
        }

        string selfPlayerId = GetSelfPlayerId();
        if (!string.IsNullOrWhiteSpace(selfPlayerId)
            && OtherPlayersOverlayPatch.TryGetSelfLocation(out int selfStage, out int selfX, out int selfY, out string _)
            && IsPlayerVisibleOnCurrentMap(mapPanel, selfStage, selfX, selfY))
        {
            NodePosKey selfPos = new(selfX, selfY);
            List<string> selfIds = GetOrCreatePlayerIdList(displayed, selfPos);
            AppendDistinct(selfIds, selfPlayerId);
        }

        return displayed;
    }

    private static bool IsPlayerVisibleOnCurrentMap(MapPanel mapPanel, int stage, int x, int y)
    {
        if (mapPanel == null || x < 0 || y < 0)
        {
            return false;
        }

        try
        {
            GameMap gameMap = Traverse.Create(mapPanel).Field("_map").GetValue<GameMap>();
            MapNode visitingNode = gameMap?.VisitingNode;
            if (visitingNode == null)
            {
                return false;
            }

            return stage < 0 || visitingNode.Act == stage;
        }
        catch
        {
            return false;
        }
    }

    private static List<string> GetOrCreatePlayerIdList(Dictionary<NodePosKey, List<string>> displayed, NodePosKey pos)
    {
        if (!displayed.TryGetValue(pos, out List<string> ids))
        {
            ids = new List<string>();
            displayed[pos] = ids;
        }

        return ids;
    }

    private static void AppendDistinct(List<string> target, IEnumerable<string> playerIds)
    {
        if (target == null || playerIds == null)
        {
            return;
        }

        foreach (string playerId in playerIds)
        {
            AppendDistinct(target, playerId);
        }
    }

    private static void AppendDistinct(List<string> target, string playerId)
    {
        if (target == null || string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        for (int i = 0; i < target.Count; i++)
        {
            if (string.Equals(target[i], playerId, StringComparison.Ordinal))
            {
                return;
            }
        }

        target.Add(playerId);
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

    private static void UpdatePlayerDots(MapNodeWidget widget, List<string> voterIds)
    {
        int count = voterIds?.Count ?? 0;
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
            Image img = ui.Dots[i];
            if (img != null)
            {
                img.color = GetPlayerDotColor(GetDotColorSeed(voterIds[i]));
            }
        }
    }

    private static string GetDotColorSeed(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return "player";
        }

        try
        {
            INetworkManager manager = TryGetNetworkManager();
            var player = manager?.GetPlayer(playerId);
            if (!string.IsNullOrWhiteSpace(player?.userName))
            {
                return player.userName;
            }

            if (!string.IsNullOrWhiteSpace(player?.playerId))
            {
                return player.playerId;
            }
        }
        catch
        {
            // ignored
        }

        return playerId;
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

        DotUi ui = new DotUi { Root = rt };
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
            Texture2D tex = new(size, size, TextureFormat.RGBA32, false);
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
            Texture2D tex = new(1, 1, TextureFormat.RGBA32, false);
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
                using JsonDocument doc = JsonDocument.Parse(s);
                root = doc.RootElement.Clone();
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

    private static string GetString(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            return p.ValueKind == JsonValueKind.String ? p.GetString() : p.GetRawText();
        }
        catch
        {
            return null;
        }
    }
}
