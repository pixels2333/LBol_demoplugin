using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Utils;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// Trade 同步补丁（Host 权威裁决 + 广播）。
///
/// 注意：服务端广播会排除发送方，因此 Host 必须在本地也执行一次 Apply。
/// 该行为与 EndTurnSyncPatch / RemoteCardUsePatch 等保持一致。
/// </summary>
public static class TradeSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static readonly object SyncLock = new();
    private static bool _subscribed;
    private static INetworkClient _subscribedClient;

    // Host 侧会话缓存（内存态；断线后可由 Snapshot 恢复/取消）。
    private static readonly Dictionary<string, TradeSessionState> _hostSessions = new(StringComparer.Ordinal);

    // Host 侧请求去重（per trade sliding window）。
    private const int RequestIdWindowSize = 64;
    private static readonly Dictionary<string, LinkedList<string>> _recentRequestIdsByTrade = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, HashSet<string>> _recentRequestIdSetsByTrade = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, long> _lastRequestTimestampByTrade = new(StringComparer.Ordinal);

    // 客户端侧最近一次状态（用于 UI 刷新）。
    private static readonly Dictionary<string, TradeSessionState> _lastKnown = new(StringComparer.Ordinal);

    public static bool IsApplyingTrade { get; private set; }

    public static event Action<TradeSessionState> OnTradeStateUpdated;

    public enum TradeStatus
    {
        Open = 0,
        Preparing = 1,
        Completed = 2,
        Canceled = 3,
        Failed = 4,
    }

    public sealed class CardRef
    {
        public string CardId { get; set; }
        public int InstanceId { get; set; }
        public bool IsUpgraded { get; set; }
        public int UpgradeCounter { get; set; }
        public int? DeckCounter { get; set; }
        public string CardName { get; set; }
        public string CardType { get; set; }
    }

    public sealed class ExhibitRef
    {
        public string ExhibitId { get; set; }
    }

    public sealed class TradeSessionState
    {
        public string TradeId { get; set; }
        public string PlayerAId { get; set; }
        public string PlayerBId { get; set; }
        public string PlayerAName { get; set; }
        public string PlayerBName { get; set; }
        public int MaxTradeSlots { get; set; }
        public TradeStatus Status { get; set; }
        public bool AConfirmed { get; set; }
        public bool BConfirmed { get; set; }
        public bool APrepared { get; set; }
        public bool BPrepared { get; set; }
        public List<CardRef> OfferA { get; set; } = new();
        public List<CardRef> OfferB { get; set; } = new();
        public int MoneyA { get; set; }
        public int MoneyB { get; set; }
        public List<ExhibitRef> ExhibitsA { get; set; } = new();
        public List<ExhibitRef> ExhibitsB { get; set; } = new();
        public string Reason { get; set; }
        public long Timestamp { get; set; }

        public bool IsParticipant(string playerId)
            => !string.IsNullOrWhiteSpace(playerId)
               && (string.Equals(PlayerAId, playerId, StringComparison.Ordinal) ||
                   string.Equals(PlayerBId, playerId, StringComparison.Ordinal));

        public TradeSessionState Clone()
            => new TradeSessionState
            {
                TradeId = TradeId,
                PlayerAId = PlayerAId,
                PlayerBId = PlayerBId,
                PlayerAName = PlayerAName,
                PlayerBName = PlayerBName,
                MaxTradeSlots = MaxTradeSlots,
                Status = Status,
                AConfirmed = AConfirmed,
                BConfirmed = BConfirmed,
                APrepared = APrepared,
                BPrepared = BPrepared,
                OfferA = OfferA?.Select(CloneCard).ToList() ?? new List<CardRef>(),
                OfferB = OfferB?.Select(CloneCard).ToList() ?? new List<CardRef>(),
                MoneyA = MoneyA,
                MoneyB = MoneyB,
                ExhibitsA = ExhibitsA?.Select(CloneExhibit).ToList() ?? new List<ExhibitRef>(),
                ExhibitsB = ExhibitsB?.Select(CloneExhibit).ToList() ?? new List<ExhibitRef>(),
                Reason = Reason,
                Timestamp = Timestamp,
            };

        private static CardRef CloneCard(CardRef c)
            => c == null
                ? null
                : new CardRef
                {
                    CardId = c.CardId,
                    InstanceId = c.InstanceId,
                    IsUpgraded = c.IsUpgraded,
                    UpgradeCounter = c.UpgradeCounter,
                    DeckCounter = c.DeckCounter,
                    CardName = c.CardName,
                    CardType = c.CardType,
                };

        private static ExhibitRef CloneExhibit(ExhibitRef e)
            => e == null
                ? null
                : new ExhibitRef
                {
                    ExhibitId = e.ExhibitId,
                };
    }

    public static void EnsureSubscribed(INetworkClient client)
    {
        if (client == null)
        {
            return;
        }

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
                _subscribedClient.OnGameEventReceived -= OnGameEventReceived;
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            client.OnGameEventReceived += OnGameEventReceived;
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

    public static INetworkClient TryGetClient()
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

    public static void RequestStartTrade(string tradeId, string playerAId, string playerBId, int maxTradeSlots)
    {
        string requestId = Guid.NewGuid().ToString("N");
        SendToHost(NetworkMessageTypes.OnTradeStartRequest, new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = requestId,
            TradeId = tradeId,
            PlayerAId = playerAId,
            PlayerBId = playerBId,
            MaxTradeSlots = maxTradeSlots,
        });
    }

    public static void RequestOfferUpdate(string tradeId, string requesterPlayerId, List<CardRef> offerCards, int moneyOffer, List<string> exhibitIds)
    {
        string requestId = Guid.NewGuid().ToString("N");
        SendToHost(NetworkMessageTypes.OnTradeOfferUpdateRequest, new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = requestId,
            TradeId = tradeId,
            RequesterPlayerId = requesterPlayerId,
            Offer = offerCards ?? new List<CardRef>(),
            Money = Math.Max(0, moneyOffer),
            Exhibits = (exhibitIds ?? new List<string>()).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray(),
        });
    }

    public static void RequestConfirm(string tradeId, string requesterPlayerId)
    {
        string requestId = Guid.NewGuid().ToString("N");
        SendToHost(NetworkMessageTypes.OnTradeConfirmRequest, new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = requestId,
            TradeId = tradeId,
            RequesterPlayerId = requesterPlayerId,
        });
    }

    public static void RequestCancel(string tradeId, string requesterPlayerId)
    {
        string requestId = Guid.NewGuid().ToString("N");
        SendToHost(NetworkMessageTypes.OnTradeCancelRequest, new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = requestId,
            TradeId = tradeId,
            RequesterPlayerId = requesterPlayerId,
        });
    }

    public static void RequestSnapshot(string tradeId, string requesterPlayerId)
    {
        string requestId = Guid.NewGuid().ToString("N");
        SendToHost(NetworkMessageTypes.OnTradeSnapshotRequest, new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = requestId,
            TradeId = tradeId,
            RequesterPlayerId = requesterPlayerId,
        });
    }

    public static void RequestPrepareResult(string tradeId, string requesterPlayerId, bool ok, string reason)
    {
        string requestId = Guid.NewGuid().ToString("N");
        SendToHost(NetworkMessageTypes.OnTradePrepareResultRequest, new
        {
            Timestamp = DateTime.UtcNow.Ticks,
            RequestId = requestId,
            TradeId = tradeId,
            RequesterPlayerId = requesterPlayerId,
            Ok = ok,
            Reason = reason,
        });
    }

    private static void SendToHost(string eventType, object payload)
    {
        try
        {
            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);
            EnsureSubscribed(client);

            client.SendGameEventData(eventType, payload);

            // Host 发起请求时不会收到服务器转发，直接在本地走一次处理。
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                OnGameEventReceived(eventType, payload);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return;
        }

        if (eventType != NetworkMessageTypes.OnTradeStartRequest &&
            eventType != NetworkMessageTypes.OnTradeOfferUpdateRequest &&
            eventType != NetworkMessageTypes.OnTradeConfirmRequest &&
            eventType != NetworkMessageTypes.OnTradeCancelRequest &&
            eventType != NetworkMessageTypes.OnTradeSnapshotRequest &&
            eventType != NetworkMessageTypes.OnTradePrepareResultRequest &&
            eventType != NetworkMessageTypes.OnTradeStateUpdate)
        {
            return;
        }

        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        switch (eventType)
        {
            case NetworkMessageTypes.OnTradeStateUpdate:
                HandleStateUpdate(root);
                return;
            default:
                HandleRequest(eventType, root);
                return;
        }
    }

    private static void HandleRequest(string eventType, JsonElement root)
    {
        // 只有 Host 处理请求。
        if (!NetworkIdentityTracker.GetSelfIsHost())
        {
            return;
        }

        string tradeId = GetString(root, "TradeId");
        if (string.IsNullOrWhiteSpace(tradeId))
        {
            return;
        }

        // RequestId 去重：避免重复请求导致状态被重复推进。
        string requestId = GetString(root, "RequestId");
        long ts = GetLong(root, "Timestamp", 0);
        if (!string.IsNullOrWhiteSpace(requestId) && IsDuplicateRequest_NoLock(tradeId, requestId, ts))
        {
            return;
        }

        switch (eventType)
        {
            case NetworkMessageTypes.OnTradeStartRequest:
                HandleStart(tradeId, root);
                return;
            case NetworkMessageTypes.OnTradeOfferUpdateRequest:
                HandleOfferUpdate(tradeId, root);
                return;
            case NetworkMessageTypes.OnTradeConfirmRequest:
                HandleConfirm(tradeId, root);
                return;
            case NetworkMessageTypes.OnTradePrepareResultRequest:
                HandlePrepareResult(tradeId, root);
                return;
            case NetworkMessageTypes.OnTradeCancelRequest:
                HandleCancel(tradeId, root);
                return;
            case NetworkMessageTypes.OnTradeSnapshotRequest:
                HandleSnapshotRequest(tradeId, root);
                return;
        }
    }

    private static void HandleStart(string tradeId, JsonElement root)
    {
        string a = GetString(root, "PlayerAId");
        string b = GetString(root, "PlayerBId");
        int maxSlots = GetInt(root, "MaxTradeSlots", 5);

        // 参与者必须在房间内（以当前 Host 侧已知玩家列表为准；为空时仅做基础校验）。
        HashSet<string> knownIds = NetworkIdentityTracker.GetPlayerIdsSnapshot();
        if (knownIds != null && knownIds.Count > 0)
        {
            if (!string.IsNullOrWhiteSpace(a) && !knownIds.Contains(a))
            {
                BroadcastState(new TradeSessionState
                {
                    TradeId = tradeId,
                    PlayerAId = a,
                    PlayerBId = b,
                    MaxTradeSlots = maxSlots,
                    Status = TradeStatus.Failed,
                    Reason = "PlayerANotInRoom",
                    Timestamp = DateTime.UtcNow.Ticks,
                });
                return;
            }

            if (!string.IsNullOrWhiteSpace(b) && !knownIds.Contains(b))
            {
                BroadcastState(new TradeSessionState
                {
                    TradeId = tradeId,
                    PlayerAId = a,
                    PlayerBId = b,
                    MaxTradeSlots = maxSlots,
                    Status = TradeStatus.Failed,
                    Reason = "PlayerBNotInRoom",
                    Timestamp = DateTime.UtcNow.Ticks,
                });
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b) || string.Equals(a, b, StringComparison.Ordinal))
        {
            BroadcastState(new TradeSessionState
            {
                TradeId = tradeId,
                PlayerAId = a,
                PlayerBId = b,
                MaxTradeSlots = maxSlots,
                Status = TradeStatus.Failed,
                Reason = "InvalidParticipants",
                Timestamp = DateTime.UtcNow.Ticks,
            });
            return;
        }

        lock (SyncLock)
        {
            if (!_hostSessions.TryGetValue(tradeId, out var state) || state == null)
            {
                state = new TradeSessionState
                {
                    TradeId = tradeId,
                    PlayerAId = a,
                    PlayerBId = b,
                    MaxTradeSlots = maxSlots,
                    Status = TradeStatus.Open,
                    Timestamp = DateTime.UtcNow.Ticks,
                };
                _hostSessions[tradeId] = state;
            }
            else
            {
                state.MaxTradeSlots = maxSlots;
                state.Timestamp = DateTime.UtcNow.Ticks;
            }

            // 确保打开交易时确认状态复位。
            state.AConfirmed = false;
            state.BConfirmed = false;
            state.APrepared = false;
            state.BPrepared = false;
            state.Status = TradeStatus.Open;
            state.Reason = null;
        }

        BroadcastState(GetHostSession(tradeId));
    }

    private static void HandleOfferUpdate(string tradeId, JsonElement root)
    {
        string requester = GetString(root, "RequesterPlayerId");
        if (string.IsNullOrWhiteSpace(requester))
        {
            return;
        }

        TradeSessionState state = GetHostSession(tradeId);
        if (state == null || state.Status != TradeStatus.Open)
        {
            return;
        }

        // Only participants can update offers.
        if (!state.IsParticipant(requester))
        {
            return;
        }

        // L1 structural validation/sanitization.
        List<CardRef> offer = SanitizeOffer(ParseOffer(root), state.MaxTradeSlots);
        int money = SanitizeMoney(GetInt(root, "Money", 0));
        List<ExhibitRef> exhibits = SanitizeExhibits(ParseExhibits(root));

        lock (SyncLock)
        {
            state = GetHostSession(tradeId);
            if (state == null || state.Status != TradeStatus.Open)
            {
                return;
            }

            // 任意报价变化都要取消双方确认，避免“确认后偷偷改报价”。
            state.AConfirmed = false;
            state.BConfirmed = false;
            state.APrepared = false;
            state.BPrepared = false;

            if (string.Equals(state.PlayerAId, requester, StringComparison.Ordinal))
            {
                state.OfferA = offer;
                state.MoneyA = money;
                state.ExhibitsA = exhibits;
            }
            else if (string.Equals(state.PlayerBId, requester, StringComparison.Ordinal))
            {
                state.OfferB = offer;
                state.MoneyB = money;
                state.ExhibitsB = exhibits;
            }
            else
            {
                return;
            }

            state.Timestamp = DateTime.UtcNow.Ticks;
        }

        BroadcastState(GetHostSession(tradeId));
    }

    private static void HandleConfirm(string tradeId, JsonElement root)
    {
        string requester = GetString(root, "RequesterPlayerId");
        if (string.IsNullOrWhiteSpace(requester))
        {
            return;
        }

        TradeSessionState state = GetHostSession(tradeId);
        if (state == null || state.Status != TradeStatus.Open)
        {
            return;
        }

        if (!state.IsParticipant(requester))
        {
            return;
        }

        lock (SyncLock)
        {
            state = GetHostSession(tradeId);
            if (state == null || state.Status != TradeStatus.Open)
            {
                return;
            }

            if (string.Equals(state.PlayerAId, requester, StringComparison.Ordinal))
            {
                state.AConfirmed = true;
            }
            else if (string.Equals(state.PlayerBId, requester, StringComparison.Ordinal))
            {
                state.BConfirmed = true;
            }
            else
            {
                return;
            }

            state.Timestamp = DateTime.UtcNow.Ticks;

            if (state.AConfirmed && state.BConfirmed)
            {
                // 两边都点了确认后，进入 Preparing：由参与者各自做本地预检，回报 ok 后才能进入 Completed。
                state.Status = TradeStatus.Preparing;
                state.APrepared = false;
                state.BPrepared = false;
                state.Reason = null;
            }
        }

        BroadcastState(GetHostSession(tradeId));
    }

    private static void HandlePrepareResult(string tradeId, JsonElement root)
    {
        string requester = GetString(root, "RequesterPlayerId");
        if (string.IsNullOrWhiteSpace(requester))
        {
            return;
        }

        bool ok = GetBool(root, "Ok");
        string reason = GetString(root, "Reason");

        TradeSessionState state = GetHostSession(tradeId);
        if (state == null || state.Status != TradeStatus.Preparing)
        {
            return;
        }

        if (!state.IsParticipant(requester))
        {
            return;
        }

        lock (SyncLock)
        {
            state = GetHostSession(tradeId);
            if (state == null || state.Status != TradeStatus.Preparing)
            {
                return;
            }

            if (string.Equals(state.PlayerAId, requester, StringComparison.Ordinal))
            {
                state.APrepared = ok;
            }
            else if (string.Equals(state.PlayerBId, requester, StringComparison.Ordinal))
            {
                state.BPrepared = ok;
            }
            else
            {
                return;
            }

            state.Timestamp = DateTime.UtcNow.Ticks;

            if (!ok)
            {
                // 允许重试：回到 Open，清空确认与准备状态。
                state.Status = TradeStatus.Open;
                state.AConfirmed = false;
                state.BConfirmed = false;
                state.APrepared = false;
                state.BPrepared = false;
                state.Reason = string.IsNullOrWhiteSpace(reason) ? "PrepareFailed" : reason;
            }
            else if (state.APrepared && state.BPrepared)
            {
                state.Status = TradeStatus.Completed;
                state.Reason = null;
            }
        }

        BroadcastState(GetHostSession(tradeId));
    }

    private static void HandleCancel(string tradeId, JsonElement root)
    {
        string requester = GetString(root, "RequesterPlayerId");
        if (string.IsNullOrWhiteSpace(requester))
        {
            return;
        }

        TradeSessionState state = GetHostSession(tradeId);
        if (state == null)
        {
            return;
        }

        lock (SyncLock)
        {
            state = GetHostSession(tradeId);
            if (state == null)
            {
                return;
            }

            if (!state.IsParticipant(requester))
            {
                return;
            }

            state.Status = TradeStatus.Canceled;
            state.Reason = "Canceled";
            state.Timestamp = DateTime.UtcNow.Ticks;
        }

        BroadcastState(GetHostSession(tradeId));
    }

    private static void HandleSnapshotRequest(string tradeId, JsonElement root)
    {
        string requester = GetString(root, "RequesterPlayerId");
        if (string.IsNullOrWhiteSpace(requester))
        {
            return;
        }

        TradeSessionState state = GetHostSession(tradeId);
        if (state == null || !state.IsParticipant(requester))
        {
            return;
        }

        // 直接回一份 state update（由服务端广播机制送达除 Host 外的参与者；Host 自己再本地 apply）。
        BroadcastState(state);
    }

    private static void BroadcastState(TradeSessionState state)
    {
        if (state == null)
        {
            return;
        }

        try
        {
            INetworkClient client = TryGetClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(client);

            var payload = new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                EventType = NetworkMessageTypes.OnTradeStateUpdate,
                TradeId = state.TradeId,
                PlayerAId = state.PlayerAId,
                PlayerBId = state.PlayerBId,
                PlayerAName = state.PlayerAName,
                PlayerBName = state.PlayerBName,
                MaxTradeSlots = state.MaxTradeSlots,
                Status = state.Status.ToString(),
                AConfirmed = state.AConfirmed,
                BConfirmed = state.BConfirmed,
                APrepared = state.APrepared,
                BPrepared = state.BPrepared,
                OfferA = state.OfferA ?? new List<CardRef>(),
                OfferB = state.OfferB ?? new List<CardRef>(),
                MoneyA = state.MoneyA,
                MoneyB = state.MoneyB,
                ExhibitsA = state.ExhibitsA ?? new List<ExhibitRef>(),
                ExhibitsB = state.ExhibitsB ?? new List<ExhibitRef>(),
                Reason = state.Reason,
            };

            client.SendGameEventData(NetworkMessageTypes.OnTradeStateUpdate, payload);

            // Host 不会收到自己的广播，补一次本地处理。
            if (NetworkIdentityTracker.GetSelfIsHost())
            {
                HandleStateUpdateFromObject(payload);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HandleStateUpdate(JsonElement root)
    {
        TradeSessionState state = ParseState(root);
        if (state == null || string.IsNullOrWhiteSpace(state.TradeId))
        {
            return;
        }

        lock (SyncLock)
        {
            _lastKnown[state.TradeId] = state;
        }

        OnTradeStateUpdated?.Invoke(state);
    }

    private static void HandleStateUpdateFromObject(object payload)
    {
        try
        {
            string json = JsonCompat.Serialize(payload);
            JsonElement root = JsonDocument.Parse(json).RootElement;
            HandleStateUpdate(root);
        }
        catch
        {
            // ignored
        }
    }

    public static TradeSessionState GetLastKnown(string tradeId)
    {
        if (string.IsNullOrWhiteSpace(tradeId))
        {
            return null;
        }

        lock (SyncLock)
        {
            _lastKnown.TryGetValue(tradeId, out var v);
            return v;
        }
    }

    private static TradeSessionState GetHostSession(string tradeId)
    {
        if (string.IsNullOrWhiteSpace(tradeId))
        {
            return null;
        }

        lock (SyncLock)
        {
            _hostSessions.TryGetValue(tradeId, out var v);
            return v;
        }
    }

    private static TradeSessionState ParseState(JsonElement root)
    {
        try
        {
            var state = new TradeSessionState
            {
                TradeId = GetString(root, "TradeId"),
                PlayerAId = GetString(root, "PlayerAId"),
                PlayerBId = GetString(root, "PlayerBId"),
                PlayerAName = GetString(root, "PlayerAName"),
                PlayerBName = GetString(root, "PlayerBName"),
                MaxTradeSlots = GetInt(root, "MaxTradeSlots", 5),
                AConfirmed = GetBool(root, "AConfirmed"),
                BConfirmed = GetBool(root, "BConfirmed"),
                APrepared = GetBool(root, "APrepared"),
                BPrepared = GetBool(root, "BPrepared"),
                Reason = GetString(root, "Reason"),
                Timestamp = GetLong(root, "Timestamp", DateTime.UtcNow.Ticks),
            };

            string statusStr = GetString(root, "Status");
            if (!Enum.TryParse(statusStr, out TradeStatus status))
            {
                status = TradeStatus.Open;
            }
            state.Status = status;

            state.OfferA = ParseOfferArray(root, "OfferA");
            state.OfferB = ParseOfferArray(root, "OfferB");

            state.MoneyA = GetInt(root, "MoneyA", 0);
            state.MoneyB = GetInt(root, "MoneyB", 0);

            state.ExhibitsA = ParseExhibitArray(root, "ExhibitsA");
            state.ExhibitsB = ParseExhibitArray(root, "ExhibitsB");

            return state;
        }
        catch
        {
            return null;
        }
    }

    private static List<CardRef> ParseOffer(JsonElement root)
    {
        // Request payload: Offer: [ ... ]
        try
        {
            return ParseOfferArray(root, "Offer");
        }
        catch
        {
            return new List<CardRef>();
        }
    }

    private static List<ExhibitRef> ParseExhibits(JsonElement root)
    {
        // Request payload: Exhibits: [ "id1", "id2" ]
        try
        {
            if (!root.TryGetProperty("Exhibits", out JsonElement arr))
            {
                return new List<ExhibitRef>();
            }

            // 新格式：字符串数组
            if (arr.ValueKind == JsonValueKind.Array)
            {
                var list = new List<ExhibitRef>();
                foreach (var e in arr.EnumerateArray())
                {
                    string id = e.ValueKind == JsonValueKind.String ? e.GetString() : e.GetRawText();
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }
                    list.Add(new ExhibitRef { ExhibitId = id });
                }

                return list
                    .Where(x => x != null && !string.IsNullOrWhiteSpace(x.ExhibitId))
                    .GroupBy(x => x.ExhibitId, StringComparer.Ordinal)
                    .Select(g => g.First())
                    .ToList();
            }
        }
        catch
        {
            // ignored
        }

        return new List<ExhibitRef>();
    }

    private static List<ExhibitRef> ParseExhibitArray(JsonElement root, string property)
    {
        var list = new List<ExhibitRef>();
        try
        {
            if (!root.TryGetProperty(property, out JsonElement arr) || arr.ValueKind != JsonValueKind.Array)
            {
                return list;
            }

            foreach (var e in arr.EnumerateArray())
            {
                string id = GetString(e, "ExhibitId");
                if (string.IsNullOrWhiteSpace(id) && e.ValueKind == JsonValueKind.String)
                {
                    id = e.GetString();
                }
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }
                list.Add(new ExhibitRef { ExhibitId = id });
            }
        }
        catch
        {
            // ignored
        }

        return list
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.ExhibitId))
            .GroupBy(x => x.ExhibitId, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();
    }

    private static List<CardRef> ParseOfferArray(JsonElement root, string property)
    {
        List<CardRef> list = new();
        try
        {
            if (!root.TryGetProperty(property, out JsonElement arr) || arr.ValueKind != JsonValueKind.Array)
            {
                return list;
            }

            foreach (JsonElement c in arr.EnumerateArray())
            {
                string cardId = GetString(c, "CardId");
                if (string.IsNullOrWhiteSpace(cardId))
                {
                    continue;
                }

                list.Add(new CardRef
                {
                    CardId = cardId,
                    InstanceId = GetInt(c, "InstanceId", -1),
                    IsUpgraded = GetBool(c, "IsUpgraded"),
                    UpgradeCounter = GetInt(c, "UpgradeCounter", 0),
                    DeckCounter = TryGetNullableInt(c, "DeckCounter"),
                    CardName = GetString(c, "CardName"),
                    CardType = GetString(c, "CardType"),
                });
            }
        }
        catch
        {
            // ignored
        }

        // 去重（InstanceId 优先；否则 CardId 兜底）。
        return list
            .GroupBy(x => x.InstanceId >= 0 ? $"i:{x.InstanceId}" : $"c:{x.CardId}")
            .Select(g => g.First())
            .ToList();
    }

    private static List<CardRef> SanitizeOffer(List<CardRef> offer, int maxSlots)
    {
        if (offer == null)
        {
            return new List<CardRef>();
        }

        // ParseOfferArray already removes empty CardId and dedupes by instance-id/card-id.
        var clean = offer
            .Where(c => c != null && !string.IsNullOrWhiteSpace(c.CardId))
            .ToList();

        if (maxSlots > 0 && clean.Count > maxSlots)
        {
            clean = clean.Take(maxSlots).ToList();
        }

        return clean;
    }

    private static int SanitizeMoney(int money)
    {
        // Keep it non-negative and within a sane game-ish upper bound.
        if (money < 0)
        {
            return 0;
        }

        return Math.Min(money, 99999);
    }

    private static List<ExhibitRef> SanitizeExhibits(List<ExhibitRef> exhibits)
    {
        if (exhibits == null)
        {
            return new List<ExhibitRef>();
        }

        return exhibits
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.ExhibitId))
            .GroupBy(x => x.ExhibitId, StringComparer.Ordinal)
            .Select(g => g.First())
            .ToList();
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

            // Host 侧 SendToHost 会把匿名对象直接回灌到本地处理，这里补一层序列化以便解析。
            if (payload != null)
            {
                string json = JsonCompat.Serialize(payload);
                root = JsonDocument.Parse(json).RootElement;
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

    private static bool IsDuplicateRequest_NoLock(string tradeId, string requestId, long timestamp)
    {
        // This method is called on the Host side; protect shared structures.
        lock (SyncLock)
        {
            if (!_recentRequestIdsByTrade.TryGetValue(tradeId, out LinkedList<string> order) || order == null)
            {
                order = new LinkedList<string>();
                _recentRequestIdsByTrade[tradeId] = order;
            }

            if (!_recentRequestIdSetsByTrade.TryGetValue(tradeId, out HashSet<string> set) || set == null)
            {
                set = new HashSet<string>(StringComparer.Ordinal);
                _recentRequestIdSetsByTrade[tradeId] = set;
            }

            if (set.Contains(requestId))
            {
                return true;
            }

            set.Add(requestId);
            order.AddLast(requestId);
            while (order.Count > RequestIdWindowSize)
            {
                string oldest = order.First?.Value;
                order.RemoveFirst();
                if (!string.IsNullOrWhiteSpace(oldest))
                {
                    set.Remove(oldest);
                }
            }

            if (timestamp > 0)
            {
                // Keep for potential future window checks / debugging.
                _lastRequestTimestampByTrade[tradeId] = Math.Max(_lastRequestTimestampByTrade.TryGetValue(tradeId, out long prev) ? prev : 0, timestamp);
            }

            return false;
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

    private static int GetInt(JsonElement elem, string property, int defaultValue)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return defaultValue;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out int i))
            {
                return i;
            }

            if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out i))
            {
                return i;
            }
        }
        catch
        {
            // ignored
        }

        return defaultValue;
    }

    private static long GetLong(JsonElement elem, string property, long defaultValue)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return defaultValue;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt64(out long l))
            {
                return l;
            }

            if (p.ValueKind == JsonValueKind.String && long.TryParse(p.GetString(), out l))
            {
                return l;
            }
        }
        catch
        {
            // ignored
        }

        return defaultValue;
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
                JsonValueKind.Number => p.TryGetInt32(out int i) && i != 0,
                JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) && b,
                _ => false,
            };
        }
        catch
        {
            return false;
        }
    }

    private static int? TryGetNullableInt(JsonElement elem, string property)
    {
        try
        {
            if (elem.ValueKind != JsonValueKind.Object || !elem.TryGetProperty(property, out JsonElement p))
            {
                return null;
            }

            if (p.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out int i))
            {
                return i;
            }

            if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out i))
            {
                return i;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    public static IDisposable EnterApplyingTradeScope()
    {
        return new ApplyingTradeScope();
    }

    private sealed class ApplyingTradeScope : IDisposable
    {
        private readonly bool _prev;

        public ApplyingTradeScope()
        {
            _prev = IsApplyingTrade;
            IsApplyingTrade = true;
        }

        public void Dispose()
        {
            IsApplyingTrade = _prev;
        }
    }
}
