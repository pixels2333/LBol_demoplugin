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

    // Confirm may arrive while the battle is still resolving animations and not yet waiting for input.
    // If we only try once, we can permanently stall with the UI gated.
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

            // Drive any deferred end-turn proceed attempts on the main thread.
            PumpPendingProceed_NoThrow();

            // 更新结束回合按钮文字（显示 X/Y 或"取消结束回合"）
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
        }
        ResetLocalTurnState();

        // If we disconnected while the local gate was holding the UI, release it.
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

    /// <summary>
    /// 检查所有在线玩家是否都已结束回合；若是则本地推进。
    /// 参考 sts2 CombatManager.AllPlayersReadyToEndTurn：每个客户端独立判断，不依赖 Host 聚合。
    /// </summary>
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

        // 所有在线玩家都结束了：设置允许推进标志，让 Prefix 放行 RequestEndPlayerTurn。
        BattleController battle = TryGetCurrentBattle();
        if (battle == null || !battle.IsWaitingPlayerInput)
        {
            // 战斗未就绪，延迟推进（由 PumpPendingProceed 在 Update 中重试）。
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
            // ignored
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
            // ignored
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
            // ignored
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

            // 重新检查是否所有玩家都结束了（可能在此期间有新玩家加入 ended）
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
            // ignored
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
            // ignored
        }
    }

    /// <summary>
    /// 更新结束回合按钮文字：本地已结束 → "取消结束回合 (X/Y)"，未结束 → 恢复原始文字。
    /// 参考 sts2 NMultiplayerPlayerState.RefreshPlayerReadyIndicator。
    /// </summary>
    private static void UpdateEndTurnButtonText()
    {
        try
        {
            // 未连接或不在联机模式时不修改
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

            // 本地已结束回合时，强制按钮保持可见且可交互（游戏自身可能隐藏它）
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

            // 尝试获取按钮文字组件（TMPro 或 UnityEngine.UI.Text）
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

                ResetLocalTurnState();

                SetEndTurnButtonInteractable(true);
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

                ResetLocalTurnState();
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

                ResetLocalTurnState();
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

                // 所有玩家都结束 → 放行真正结束回合
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
                    // 回合结束后由 StartPlayerTurn postfix 重置 _endedPlayers
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
                    // 已结束回合 → 取消结束回合（参考 sts2 UndoReadyToEndTurn）
                    lock (_syncLock)
                    {
                        _localEndedTurn = false;
                        _endedPlayers.Remove(selfPlayerId);
                    }

                    try
                    {
                        client.SendGameEventData("EndTurnCancel", new
                        {
                            Timestamp = DateTime.Now.Ticks,
                            PlayerId = selfPlayerId,
                        });
                    }
                    catch
                    {
                        // ignored
                    }

                    RefreshAllCardsEdge();
                    Plugin.Logger?.LogInfo($"[EndTurnSync] Cancelled end turn: {selfPlayerId}");
                    return false;
                }

                // 结束回合
                string battleId = GetBattleId(__instance);
                int round = __instance.RoundCounter;

                lock (_syncLock)
                {
                    _localEndedTurn = true;
                    _pendingBattleId = battleId;
                    _pendingRound = round;
                    _endedPlayers.Add(selfPlayerId);
                }

                // 按钮保持可交互（用于取消），不设为不可点击
                RefreshAllCardsEdge();

                try
                {
                    client.SendGameEventData(NetworkMessageTypes.EndTurnRequest, new
                    {
                        Timestamp = DateTime.Now.Ticks,
                        PlayerId = selfPlayerId,
                        BattleId = battleId,
                        Round = round,
                    });
                }
                catch
                {
                    // ignored
                }

                // 检查是否所有玩家都结束了（单玩家时直接推进）
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
