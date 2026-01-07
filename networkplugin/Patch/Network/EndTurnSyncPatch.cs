using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 回合结束同步补丁（参考 Together in Spire 的 EndTurnPatches）：
/// - 玩家点击“结束回合”时不立刻结束，而是上报“已结束回合”并锁定本地操作；
/// - 等待房主/服务器确认“所有玩家都结束回合”，再允许真正进入敌方回合。
///
/// 说明：当前实现采用“房主客户端侧聚合 EndTurnRequest 并广播确认”的策略，
/// 前提是服务器会将 SendGameEventData 广播给所有客户端（含发送方）。
/// </summary>
[HarmonyPatch]
public static class EndTurnSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool _subscribed;
    private static INetworkClient _subscribedClient;
    private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;
    private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

    private static readonly object _syncLock = new();
    private static string _selfPlayerId;
    private static bool _selfIsHost;
    private static HashSet<string> _activePlayerIds = new(StringComparer.Ordinal);

    private static bool _localEndedTurn;
    private static bool _allowEndTurn;
    private static readonly HashSet<string> _endedPlayers = new(StringComparer.Ordinal);
    private static string _pendingBattleId;
    private static int _pendingRound = -1;
    private static string _lastConfirmedBattleId;
    private static int _lastConfirmedRound = -1;

    public static bool LocalEndedTurn
    {
        get
        {
            lock (_syncLock)
            {
                return _localEndedTurn;
            }
        }
    }

    private static INetworkClient TryGetNetworkClient()
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

    [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient client = TryGetNetworkClient();
            if (client == null)
            {
                return;
            }

            EnsureSubscribed(client);
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
            // ignored
        }

        try
        {
            client.OnGameEventReceived += _onGameEventReceived;
            client.OnConnectionStateChanged += _onConnectionStateChanged;
            _subscribedClient = client;
            _subscribed = true;
        }
        catch
        {
            _subscribedClient = null;
            _subscribed = false;
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
            _selfPlayerId = null;
            _selfIsHost = false;
            _activePlayerIds = new HashSet<string>(StringComparer.Ordinal);
            _endedPlayers.Clear();
            _localEndedTurn = false;
            _allowEndTurn = false;
            _pendingBattleId = null;
            _pendingRound = -1;
            _lastConfirmedBattleId = null;
            _lastConfirmedRound = -1;
        }
    }

    private static void OnGameEventReceived(string eventType, object payload)
    {
        if (!TryGetJsonElement(payload, out JsonElement root))
        {
            return;
        }

        switch (eventType)
        {
            case NetworkMessageTypes.Welcome:
                HandleWelcome(root);
                return;
            case NetworkMessageTypes.HostChanged:
                HandleHostChanged(root);
                return;
            case NetworkMessageTypes.PlayerListUpdate:
                HandlePlayerListUpdate(root);
                return;
            case NetworkMessageTypes.PlayerJoined:
                HandlePlayerJoined(root);
                return;
            case NetworkMessageTypes.PlayerLeft:
                HandlePlayerLeft(root);
                return;
            case NetworkMessageTypes.EndTurnRequest:
                HandleEndTurnRequest(root);
                return;
            case NetworkMessageTypes.EndTurnStatus:
                HandleEndTurnStatus(root);
                return;
            case NetworkMessageTypes.EndTurnConfirm:
                HandleEndTurnConfirm(root);
                return;
        }
    }

    private static void HandleWelcome(JsonElement root)
    {
        try
        {
            string playerId = GetString(root, "PlayerId");
            bool isHost = GetBool(root, "IsHost");

            HashSet<string> activeIds = new(StringComparer.Ordinal);
            JsonElement playersElem;
            bool hasPlayers = root.TryGetProperty("Players", out playersElem) && playersElem.ValueKind == JsonValueKind.Array;
            if (!hasPlayers)
            {
                // NetworkServer.Welcome 使用 PlayerList 字段
                hasPlayers = root.TryGetProperty("PlayerList", out playersElem) && playersElem.ValueKind == JsonValueKind.Array;
            }

            if (hasPlayers)
            {
                foreach (JsonElement p in playersElem.EnumerateArray())
                {
                    string id = GetString(p, "PlayerId");
                    bool isConnected = p.ValueKind == JsonValueKind.Object && p.TryGetProperty("IsConnected", out JsonElement c)
                        ? (c.ValueKind == JsonValueKind.True || (c.ValueKind == JsonValueKind.String && bool.TryParse(c.GetString(), out bool cb) && cb))
                        : true;
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        // “等所有玩家同意”默认只统计在线玩家；离线玩家不阻塞回合推进
                        if (isConnected)
                        {
                            activeIds.Add(id);
                        }
                    }
                }
            }

            lock (_syncLock)
            {
                _selfPlayerId = playerId;
                _selfIsHost = isHost;
                _activePlayerIds = activeIds;
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HandleHostChanged(JsonElement root)
    {
        try
        {
            string newHostId = GetString(root, "NewHostId");
            if (string.IsNullOrWhiteSpace(newHostId))
            {
                return;
            }

            lock (_syncLock)
            {
                _selfIsHost = string.Equals(_selfPlayerId, newHostId, StringComparison.Ordinal);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void HandlePlayerListUpdate(JsonElement root)
    {
        if (!root.TryGetProperty("Players", out JsonElement playersElem) || playersElem.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        HashSet<string> activeIds = new(StringComparer.Ordinal);
        foreach (JsonElement p in playersElem.EnumerateArray())
        {
            string id = GetString(p, "PlayerId");
            if (!string.IsNullOrWhiteSpace(id))
            {
                bool isConnected = p.ValueKind == JsonValueKind.Object && p.TryGetProperty("IsConnected", out JsonElement c)
                    ? (c.ValueKind == JsonValueKind.True || (c.ValueKind == JsonValueKind.String && bool.TryParse(c.GetString(), out bool cb) && cb))
                    : true;
                if (isConnected)
                {
                    activeIds.Add(id);
                }
            }
        }

        lock (_syncLock)
        {
            _activePlayerIds = activeIds;
            _endedPlayers.RemoveWhere(pid => !activeIds.Contains(pid));
        }
    }

    private static void HandlePlayerJoined(JsonElement root)
    {
        string id = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        lock (_syncLock)
        {
            bool isConnected = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("IsConnected", out JsonElement c)
                ? (c.ValueKind == JsonValueKind.True || (c.ValueKind == JsonValueKind.String && bool.TryParse(c.GetString(), out bool cb) && cb))
                : true;
            if (isConnected)
            {
                _activePlayerIds.Add(id);
            }
        }
    }

    private static void HandlePlayerLeft(JsonElement root)
    {
        string id = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        lock (_syncLock)
        {
            _activePlayerIds.Remove(id);
            _endedPlayers.Remove(id);
        }
    }

    private static void HandleEndTurnRequest(JsonElement root)
    {
        if (!IsSelfHost())
        {
            return;
        }

        string playerId = GetString(root, "PlayerId");
        string battleId = GetString(root, "BattleId");
        int round = GetInt(root, "Round", -1);
        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(battleId) || round < 0)
        {
            return;
        }

        MarkPlayerEnded(playerId, battleId, round, true, broadcastStatusFromHost: true);
    }

    private static void HandleEndTurnStatus(JsonElement root)
    {
        string playerId = GetString(root, "PlayerId");
        string battleId = GetString(root, "BattleId");
        int round = GetInt(root, "Round", -1);
        bool ended = GetBool(root, "Ended");
        if (string.IsNullOrWhiteSpace(playerId) || string.IsNullOrWhiteSpace(battleId) || round < 0)
        {
            return;
        }

        MarkPlayerEnded(playerId, battleId, round, ended, broadcastStatusFromHost: false);
    }

    private static void HandleEndTurnConfirm(JsonElement root)
    {
        string battleId = GetString(root, "BattleId");
        int round = GetInt(root, "Round", -1);
        if (string.IsNullOrWhiteSpace(battleId) || round < 0)
        {
            return;
        }

        TryProceedAfterConfirm(battleId, round);
    }

    private static void TryProceedAfterConfirm(string battleId, int round)
    {
        bool shouldProceed;
        lock (_syncLock)
        {
            shouldProceed = _localEndedTurn &&
                            string.Equals(_pendingBattleId, battleId, StringComparison.Ordinal) &&
                            _pendingRound == round;
            if (shouldProceed)
            {
                _allowEndTurn = true;
            }
        }

        if (!shouldProceed)
        {
            return;
        }

        BattleController battle = TryGetCurrentBattle();
        if (battle == null || !battle.IsWaitingPlayerInput)
        {
            return;
        }

        try
        {
            battle.RequestEndPlayerTurn();
        }
        catch
        {
            // ignored
        }
    }

    private static void MarkPlayerEnded(string playerId, string battleId, int round, bool ended, bool broadcastStatusFromHost)
    {
        INetworkClient client = TryGetNetworkClient();
        if (client == null || !client.IsConnected)
        {
            return;
        }

        bool shouldBroadcastStatus = false;
        bool shouldConfirm = false;
        int playerCountSnapshot = 0;

        lock (_syncLock)
        {
            if (ended)
            {
                _endedPlayers.Add(playerId);
            }
            else
            {
                _endedPlayers.Remove(playerId);
            }

            if (_selfIsHost)
            {
                shouldBroadcastStatus = broadcastStatusFromHost;

                playerCountSnapshot = _activePlayerIds.Count;
                if (playerCountSnapshot > 0 &&
                    _endedPlayers.IsSupersetOf(_activePlayerIds) &&
                    !(string.Equals(_lastConfirmedBattleId, battleId, StringComparison.Ordinal) && _lastConfirmedRound == round))
                {
                    shouldConfirm = true;
                    _lastConfirmedBattleId = battleId;
                    _lastConfirmedRound = round;
                }
            }
        }

        if (shouldBroadcastStatus)
        {
            try
            {
                client.SendGameEventData(NetworkMessageTypes.EndTurnStatus, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    PlayerId = playerId,
                    BattleId = battleId,
                    Round = round,
                    Ended = ended,
                });
            }
            catch
            {
                // ignored
            }
        }

        if (!shouldConfirm)
        {
            return;
        }

        try
        {
            client.SendGameEventData(NetworkMessageTypes.EndTurnConfirm, new
            {
                Timestamp = DateTime.Now.Ticks,
                BattleId = battleId,
                Round = round,
                PlayerCount = playerCountSnapshot
            });
        }
        catch
        {
            // ignored
        }

        // 服务器广播 GameEvent 会排除发送方：房主不会收到自己广播的确认，因此需要本地推进。
        if (IsSelfHost())
        {
            TryProceedAfterConfirm(battleId, round);
        }
    }

    private static bool IsSelfHost()
    {
        lock (_syncLock)
        {
            return _selfIsHost;
        }
    }

    private static string GetEffectivePlayerId()
    {
        lock (_syncLock)
        {
            if (!string.IsNullOrWhiteSpace(_selfPlayerId))
            {
                return _selfPlayerId;
            }
        }

        return null;
    }

    private static string GetBattleId(BattleController battle)
    {
        try
        {
            var run = GameStateUtils.GetCurrentGameRun();
            var node = run?.CurrentMap?.VisitingNode;
            if (node != null)
            {
                // act/x/y/站点类型：在同一局中对所有客户端应一致
                return $"Act{node.Act}:{node.X}:{node.Y}:{node.StationType}";
            }
        }
        catch
        {
            // ignored
        }

        return "battle";
    }

    private static BattleController TryGetCurrentBattle()
    {
        try
        {
            var playBoard = UiManager.GetPanel<PlayBoard>();
            if (playBoard == null)
            {
                return null;
            }

            return Traverse.Create(playBoard).Property("Battle").GetValue<BattleController>();
        }
        catch
        {
            return null;
        }
    }

    private static void RefreshAllCardsEdge()
    {
        try
        {
            var playBoard = UiManager.GetPanel<PlayBoard>();
            if (playBoard == null)
            {
                return;
            }

            var cardUi = Traverse.Create(playBoard).Field("cardUi").GetValue<CardUi>();
            cardUi?.RefreshAllCardsEdge();
        }
        catch
        {
            // ignored
        }
    }

    private static void ForceDisableEndTurnButton()
    {
        try
        {
            var playBoard = UiManager.GetPanel<PlayBoard>();
            if (playBoard == null)
            {
                return;
            }

            var endTurnButton = Traverse.Create(playBoard).Field("endTurnButton").GetValue<UnityEngine.UI.Button>();
            if (endTurnButton != null)
            {
                endTurnButton.interactable = false;
            }
        }
        catch
        {
            // ignored
        }
    }

    private static bool ShouldSync(BattleController battle)
        => battle != null && battle.Player != null && battle.Player == GameStateUtils.GetCurrentPlayer();

    [HarmonyPatch(typeof(BattleController), "StartPlayerTurn")]
    private static class BattleController_StartPlayerTurn_Reset
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                lock (_syncLock)
                {
                    _endedPlayers.Clear();
                    _localEndedTurn = false;
                    _allowEndTurn = false;
                    _pendingBattleId = null;
                    _pendingRound = -1;
                    _lastConfirmedBattleId = null;
                    _lastConfirmedRound = -1;
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "StartBattle")]
    private static class BattleController_StartBattle_Reset
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                lock (_syncLock)
                {
                    _endedPlayers.Clear();
                    _localEndedTurn = false;
                    _allowEndTurn = false;
                    _pendingBattleId = null;
                    _pendingRound = -1;
                    _lastConfirmedBattleId = null;
                    _lastConfirmedRound = -1;
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "EndBattle")]
    private static class BattleController_EndBattle_Reset
    {
        [HarmonyPostfix]
        public static void Postfix(BattleController __instance)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                lock (_syncLock)
                {
                    _endedPlayers.Clear();
                    _localEndedTurn = false;
                    _allowEndTurn = false;
                    _pendingBattleId = null;
                    _pendingRound = -1;
                    _lastConfirmedBattleId = null;
                    _lastConfirmedRound = -1;
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.RequestEndPlayerTurn))]
    private static class BattleController_RequestEndPlayerTurn_Gate
    {
        [HarmonyPrefix]
        public static bool Prefix(BattleController __instance)
        {
            try
            {
                INetworkClient client = TryGetNetworkClient();
                if (client == null || !client.IsConnected)
                {
                    return true;
                }

                if (!ShouldSync(__instance))
                {
                    return true;
                }

                bool allowNow;
                lock (_syncLock)
                {
                    allowNow = _allowEndTurn;
                }

                if (allowNow)
                {
                    lock (_syncLock)
                    {
                        _allowEndTurn = false;
                        _localEndedTurn = false;
                        _pendingBattleId = null;
                        _pendingRound = -1;
                    }

                    return true;
                }

                lock (_syncLock)
                {
                    if (_localEndedTurn)
                    {
                        return false;
                    }
                }

                string battleId = GetBattleId(__instance);
                int round = __instance.RoundCounter;

                string selfPlayerId = GetEffectivePlayerId();
                if (string.IsNullOrWhiteSpace(selfPlayerId))
                {
                    // 未拿到 Welcome.PlayerId 时不介入，避免把回合逻辑锁死
                    return true;
                }

                lock (_syncLock)
                {
                    _localEndedTurn = true;
                    _pendingBattleId = battleId;
                    _pendingRound = round;
                    _endedPlayers.Add(selfPlayerId);
                }

                ForceDisableEndTurnButton();
                RefreshAllCardsEdge();

                client.SendGameEventData(NetworkMessageTypes.EndTurnRequest, new
                {
                    Timestamp = DateTime.Now.Ticks,
                    PlayerId = selfPlayerId,
                    BattleId = battleId,
                    Round = round,
                });

                // 服务器广播 GameEvent 会排除发送方：房主需要在本地把自己也计入聚合并可能触发确认。
                if (IsSelfHost())
                {
                    MarkPlayerEnded(selfPlayerId, battleId, round, true, broadcastStatusFromHost: true);
                }

                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EndTurnSync] Error in RequestEndPlayerTurn prefix: {ex.Message}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Card), "get_CanUse")]
    private static class Card_CanUse_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (!LocalEndedTurn)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(UltimateSkill), "get_Available")]
    private static class UltimateSkill_Available_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (!LocalEndedTurn)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Doll), "get_Usable")]
    private static class Doll_Usable_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (!LocalEndedTurn)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayBoard), "UseUsVerify", new[] { typeof(UnitSelector) })]
    private static class PlayBoard_UseUsVerify_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (!LocalEndedTurn)
            {
                return true;
            }

            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayBoard), "UseDollVerify", new[] { typeof(Doll), typeof(UnitSelector) })]
    private static class PlayBoard_UseDollVerify_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            if (!LocalEndedTurn)
            {
                return true;
            }

            __result = false;
            return false;
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

            if (p.ValueKind == JsonValueKind.String && int.TryParse(p.GetString(), out int s))
            {
                return s;
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
                JsonValueKind.String => bool.TryParse(p.GetString(), out bool b) && b,
                _ => false,
            };
        }
        catch
        {
            return false;
        }
    }
}
