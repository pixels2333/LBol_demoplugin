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
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;
using TMPro;

namespace NetworkPlugin.Patch.Network;

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

    private static string _pendingProceedBattleId;
    private static int _pendingProceedRound = -1;
    private static long _pendingProceedStartUtcTicks;
    private static int _pendingProceedAttempts;
    private const int PendingProceedTimeoutMs = 2500;

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

    private static void ResetLocalTurnState()
    {
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

    private static bool AllowActionBeforeTurnEnds(ref bool __result)
    {
        if (!LocalEndedTurn)
        {
            return true;
        }

        __result = false;
        return false;
    }

    private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

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

            PumpPendingProceed_NoThrow();

            UpdateEndTurnButtonText();
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
        }
        ResetLocalTurnState();

        SetEndTurnButtonInteractable(true);
        RefreshAllCardsEdge();
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
            case "EndTurnCancel":
                HandleEndTurnCancel(root);
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
        string playerId = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (_syncLock)
        {
            _endedPlayers.Add(playerId);
        }

        CheckAllPlayersEnded();
    }

    private static void HandleEndTurnCancel(JsonElement root)
    {
        string playerId = GetString(root, "PlayerId");
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return;
        }

        lock (_syncLock)
        {
            _endedPlayers.Remove(playerId);
        }
    }

        private static void CheckAllPlayersEnded()
    {
        bool allEnded;
        int totalCount;
        int endedCount;
        string pendingBattleId;
        int pendingRound;

        lock (_syncLock)
        {
            totalCount = _activePlayerIds.Count;
            endedCount = _endedPlayers.Count;
            allEnded = totalCount > 0 && _endedPlayers.IsSupersetOf(_activePlayerIds);
            pendingBattleId = _pendingBattleId;
            pendingRound = _pendingRound;
        }

        if (!allEnded)
        {
            return;
        }

        BattleController battle = TryGetCurrentBattle();
        if (battle == null || !battle.IsWaitingPlayerInput)
        {

            SchedulePendingProceed_NoThrow(pendingBattleId ?? "battle", pendingRound, battle == null ? "battle_null" : "not_waiting_input");
            return;
        }

        lock (_syncLock)
        {
            _allowEndTurn = true;
        }

        try
        {
            battle.RequestEndPlayerTurn();
        }
        catch
        {

        }
    }

    private static void SchedulePendingProceed_NoThrow(string battleId, int round, string reason)
    {
        try
        {
            bool changed = false;
            lock (_syncLock)
            {
                if (!string.Equals(_pendingProceedBattleId, battleId, StringComparison.Ordinal) || _pendingProceedRound != round)
                {
                    changed = true;
                    _pendingProceedBattleId = battleId;
                    _pendingProceedRound = round;
                    _pendingProceedStartUtcTicks = DateTime.UtcNow.Ticks;
                    _pendingProceedAttempts = 0;
                }
            }

            if (changed)
            {
                Plugin.Logger?.LogDebug($"[EndTurnSync] Proceed deferred ({reason}): battleId={battleId}, round={round}");
            }
        }
        catch
        {

        }
    }

    private static void ClearPendingProceed_NoThrow()
    {
        try
        {
            lock (_syncLock)
            {
                _pendingProceedBattleId = null;
                _pendingProceedRound = -1;
                _pendingProceedStartUtcTicks = 0;
                _pendingProceedAttempts = 0;
            }
        }
        catch
        {

        }
    }

    private static void PumpPendingProceed_NoThrow()
    {
        try
        {
            string battleId;
            int round;
            long startUtcTicks;
            int attempts;

            lock (_syncLock)
            {
                battleId = _pendingProceedBattleId;
                round = _pendingProceedRound;
                startUtcTicks = _pendingProceedStartUtcTicks;
                attempts = _pendingProceedAttempts;
            }

            if (string.IsNullOrWhiteSpace(battleId) || round < 0)
            {
                return;
            }

            double elapsedMs = 0;
            if (startUtcTicks > 0)
            {
                elapsedMs = new TimeSpan(DateTime.UtcNow.Ticks - startUtcTicks).TotalMilliseconds;
            }

            if (elapsedMs > PendingProceedTimeoutMs || attempts > 300)
            {
                Plugin.Logger?.LogWarning($"[EndTurnSync] Proceed timeout: battleId={battleId}, round={round}, elapsedMs={(int)elapsedMs}");
                ClearPendingProceed_NoThrow();
                return;
            }

            lock (_syncLock)
            {
                _pendingProceedAttempts++;
            }

            bool allEnded;
            lock (_syncLock)
            {
                allEnded = _activePlayerIds.Count > 0 && _endedPlayers.IsSupersetOf(_activePlayerIds);
            }

            if (!allEnded)
            {
                ClearPendingProceed_NoThrow();
                return;
            }

            BattleController battle = TryGetCurrentBattle();
            if (battle == null || !battle.IsWaitingPlayerInput)
            {
                return;
            }

            lock (_syncLock)
            {
                _allowEndTurn = true;
            }

            battle.RequestEndPlayerTurn();
            ClearPendingProceed_NoThrow();
        }
        catch
        {

        }
    }

    private static bool IsSelfHost()
    {
        lock (_syncLock)
        {
            return _selfIsHost;
        }
    }

    private static string GetBattleId(BattleController battle)
    {
        try
        {
            var run = GameStateUtils.GetCurrentGameRun();
            var node = run?.CurrentMap?.VisitingNode;
            if (node != null)
            {

                return $"Act{node.Act}:{node.X}:{node.Y}:{node.StationType}";
            }
        }
        catch
        {

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

        }
    }

    private static void SetEndTurnButtonInteractable(bool interactable)
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
                endTurnButton.interactable = interactable;
            }
        }
        catch
        {

        }
    }

        private static void UpdateEndTurnButtonText()
    {
        try
        {

            INetworkClient client = TryGetNetworkClient();
            if (client == null || !client.IsConnected)
            {
                return;
            }

            var playBoard = UiManager.GetPanel<PlayBoard>();
            if (playBoard == null)
            {
                return;
            }

            var endTurnButton = Traverse.Create(playBoard).Field("endTurnButton").GetValue<UnityEngine.UI.Button>();
            if (endTurnButton == null)
            {
                return;
            }

            bool localEnded;
            int totalCount;
            int endedCount;
            lock (_syncLock)
            {
                localEnded = _localEndedTurn;
                totalCount = _activePlayerIds.Count;
                endedCount = _endedPlayers.Count;
            }

            if (localEnded)
            {
                if (!endTurnButton.gameObject.activeSelf)
                {
                    endTurnButton.gameObject.SetActive(true);
                }
                if (!endTurnButton.interactable)
                {
                    endTurnButton.interactable = true;
                }
            }

            var textComponent = endTurnButton.GetComponentInChildren<TMPro.TMP_Text>(true);
            string targetText = localEnded
                ? $"取消结束回合 ({endedCount}/{totalCount})"
                : (endedCount > 0 ? $"结束回合 ({endedCount}/{totalCount})" : "结束回合");

            if (textComponent != null)
            {
                textComponent.text = targetText;
            }
            else
            {
                var uiText = endTurnButton.GetComponentInChildren<UnityEngine.UI.Text>(true);
                if (uiText != null)
                {
                    uiText.text = targetText;
                }
            }
        }
        catch
        {

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

                ResetLocalTurnState();

                SetEndTurnButtonInteractable(true);
            }
            catch
            {

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

                ResetLocalTurnState();
            }
            catch
            {

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

                ResetLocalTurnState();
            }
            catch
            {

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

                string selfPlayerId;
                lock (_syncLock)
                {
                    selfPlayerId = _selfPlayerId;
                }
                if (string.IsNullOrWhiteSpace(selfPlayerId))
                {
                    return true;
                }

                bool alreadyEnded;
                lock (_syncLock)
                {
                    alreadyEnded = _localEndedTurn;
                }

                if (alreadyEnded)
                {

                    lock (_syncLock)
                    {
                        _localEndedTurn = false;
                        _endedPlayers.Remove(selfPlayerId);
                    }

                    try
                    {
                        client.BroadcastState("EndTurnCancel", new
                        {
                            Timestamp = DateTime.Now.Ticks,
                            PlayerId = selfPlayerId,
                        });
                    }
                    catch
                    {

                    }

                    RefreshAllCardsEdge();
                    Plugin.Logger?.LogInfo($"[EndTurnSync] Cancelled end turn: {selfPlayerId}");
                    return false;
                }

                string battleId = GetBattleId(__instance);
                int round = __instance.RoundCounter;

                lock (_syncLock)
                {
                    _localEndedTurn = true;
                    _pendingBattleId = battleId;
                    _pendingRound = round;
                    _endedPlayers.Add(selfPlayerId);
                }

                RefreshAllCardsEdge();

                try
                {
                    client.BroadcastState(NetworkMessageTypes.EndTurnRequest, new
                    {
                        Timestamp = DateTime.Now.Ticks,
                        PlayerId = selfPlayerId,
                        BattleId = battleId,
                        Round = round,
                    });
                }
                catch
                {

                }

                CheckAllPlayersEnded();

                return false;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EndTurnSync] Error in RequestEndPlayerTurn prefix: {ex.Message}");
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Card), nameof(Card.CanUse), MethodType.Getter)]
    private static class Card_CanUse_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            return AllowActionBeforeTurnEnds(ref __result);
        }
    }

    [HarmonyPatch(typeof(UltimateSkill), nameof(UltimateSkill.Available), MethodType.Getter)]
    private static class UltimateSkill_Available_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            return AllowActionBeforeTurnEnds(ref __result);
        }
    }

    [HarmonyPatch(typeof(Doll), nameof(Doll.Usable), MethodType.Getter)]
    private static class Doll_Usable_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            return AllowActionBeforeTurnEnds(ref __result);
        }
    }

    [HarmonyPatch(typeof(PlayBoard), "UseUsVerify", new[] { typeof(UnitSelector) })]
    private static class PlayBoard_UseUsVerify_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            return AllowActionBeforeTurnEnds(ref __result);
        }
    }

    [HarmonyPatch(typeof(PlayBoard), "UseDollVerify", new[] { typeof(Doll), typeof(UnitSelector) })]
    private static class PlayBoard_UseDollVerify_BlockAfterEnd
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            return AllowActionBeforeTurnEnds(ref __result);
        }
    }

    private static bool TryGetJsonElement(object payload, out JsonElement root)
        => NetworkEventHelper.TryGetJsonElement(payload, out root);

    private static string GetString(JsonElement root, string name)
        => NetworkEventHelper.GetString(root, name);

    private static int GetInt(JsonElement elem, string property, int defaultValue)
    {
        if (NetworkEventHelper.TryGetInt(elem, property, out int v))
            return v;
        return defaultValue;
    }

    private static bool GetBool(JsonElement root, string name)
        => NetworkEventHelper.GetBool(root, name);
}
