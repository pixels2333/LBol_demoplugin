using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Presentation.Units;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

public static partial class RemoteCardUsePatch
{
    #region Receive (subscribe + UI hint)

        private static bool _subscribed;

        private static INetworkClient _subscribedClient;

        private static readonly Action<string, object> _onGameEventReceived = OnGameEventReceived;

        private static readonly Action<bool> _onConnectionStateChanged = OnConnectionStateChanged;

        private static readonly object _syncLock = new();

        private static string _selfPlayerId;

        [HarmonyPatch(typeof(GameDirector), "Update")]
    private static class SubscribeHook
    {
                [HarmonyPostfix]
        public static void Postfix()
        {
            INetworkClient client = TryGetClient();
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
        }

        lock (_resolvedLock)
        {
            _lastResolvedSeqByTarget.Clear();
            _lastResolvedTimestampByTarget.Clear();
            _processedResolvedRequestIdsByTarget.Clear();
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
            case NetworkMessageTypes.OnRemoteCardUse:
                HandleRemoteCardUse(root);
                return;
            case NetworkMessageTypes.OnRemoteCardResolved:
                HandleRemoteCardResolved(root);
                return;
        }
    }

        private static void HandleWelcome(JsonElement root)
    {
        try
        {
            if (!root.TryGetProperty("PlayerId", out JsonElement idEl) || idEl.ValueKind != JsonValueKind.String)
            {
                return;
            }

            string pid = idEl.GetString();
            lock (_syncLock)
            {
                _selfPlayerId = pid;
            }
        }
        catch
        {

        }
    }

        private static void HandleRemoteCardUse(JsonElement root)
    {
        try
        {
            string targetId = GetString(root, "TargetPlayerId");
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            string selfId;
            lock (_syncLock)
            {
                selfId = _selfPlayerId;
            }

            string senderName = GetString(root, "SenderName") ?? "Remote";

            string cardName = null;
            if (root.TryGetProperty("Card", out JsonElement cardEl) && cardEl.ValueKind == JsonValueKind.Object)
            {
                cardName = GetString(cardEl, "CardName") ?? GetString(cardEl, "CardId");
            }

            GameRunController run = GameStateUtils.GetCurrentGameRun();
            BattleController battle = run?.Battle;

            bool isExecutingClient = false;
            string targetUnitKind = GetString(root, "TargetUnitKind");
            if (string.Equals(targetUnitKind, "Enemy", StringComparison.OrdinalIgnoreCase))
            {
                if (NetworkIdentityTracker.GetSelfIsHost())
                {
                    isExecutingClient = true;
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, targetId, StringComparison.Ordinal))
                {
                    isExecutingClient = true;
                }
            }

            if (battle != null)
            {
                if (root.TryGetProperty("Actions", out JsonElement actionsEl))
                {
                    Singleton<LBoL.Presentation.Units.GameDirector>.Instance?.StartCoroutine(PlayVisualsCoroutine(actionsEl.Clone(), battle, isExecutingClient));
                }
            }

            if (isExecutingClient)
            {
                if (string.Equals(targetUnitKind, "Enemy", StringComparison.OrdinalIgnoreCase))
                {
                    ShowTopMessage($"{senderName} used {cardName ?? "a card"} on enemy.");
                }
                else
                {
                    ShowTopMessage($"{senderName} used {cardName ?? "a card"} on you.");
                }
                TryExecuteRemoteCardUse(root);
            }
            else
            {
                ShowTopMessage($"{senderName} used {cardName ?? "a card"}.");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Receive failed: {ex.Message}");
        }
    }

        private static void HandleRemoteCardResolved(JsonElement root)
    {
        try
        {
            string playerId = GetString(root, "TargetPlayerId");
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return;
            }

            if (!ShouldApplyRemoteResolved(playerId, root))
            {
                return;
            }

            ApplyRemoteResolvedStateToView(playerId, root);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[RemoteCardUse] Resolve receive failed: {ex.Message}");
        }
    }

    private static bool ShouldApplyRemoteResolved(string targetPlayerId, JsonElement root)
    {
        try
        {
            long resolveSeq = GetLong(root, "ResolveSeq") ?? 0;
            long timestamp = GetLong(root, "Timestamp") ?? 0;
            string requestId = GetString(root, "RequestId");

            lock (_resolvedLock)
            {
                if (!string.IsNullOrWhiteSpace(requestId))
                {
                    if (!_processedResolvedRequestIdsByTarget.TryGetValue(targetPlayerId, out HashSet<string> set))
                    {
                        set = new HashSet<string>(StringComparer.Ordinal);
                        _processedResolvedRequestIdsByTarget[targetPlayerId] = set;
                    }

                    if (set.Contains(requestId))
                    {
                        return false;
                    }

                    set.Add(requestId);
                    if (set.Count > 256)
                    {
                        set.Clear();
                        set.Add(requestId);
                    }
                }

                if (resolveSeq > 0)
                {
                    if (_lastResolvedSeqByTarget.TryGetValue(targetPlayerId, out long lastSeq) && resolveSeq <= lastSeq)
                    {
                        return false;
                    }

                    _lastResolvedSeqByTarget[targetPlayerId] = resolveSeq;
                    if (timestamp > 0)
                    {
                        _lastResolvedTimestampByTarget[targetPlayerId] = timestamp;
                    }
                    return true;
                }

                if (timestamp > 0)
                {
                    if (_lastResolvedTimestampByTarget.TryGetValue(targetPlayerId, out long lastTs) && timestamp <= lastTs)
                    {
                        return false;
                    }

                    _lastResolvedTimestampByTarget[targetPlayerId] = timestamp;
                    return true;
                }
            }

            return true;
        }
        catch
        {
            return true;
        }
    }

        private static void TryPlayRemoteCardUseAnimation(JsonElement root)
    {
        try
        {
            string senderId = GetString(root, "SenderPlayerId");
            string targetId = GetString(root, "TargetPlayerId");

            string cardType = null;
            if (root.TryGetProperty("Card", out JsonElement cardEl) && cardEl.ValueKind == JsonValueKind.Object)
            {
                cardType = GetString(cardEl, "CardType");
            }

            string anim = MapCardTypeToAnimation(cardType);

            string selfId;
            lock (_syncLock)
            {
                selfId = _selfPlayerId;
            }

            if (!string.IsNullOrWhiteSpace(senderId) && OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(senderId, out UnitView casterView))
            {
                casterView.PlayAnimation(anim);
            }
            else if (!string.IsNullOrWhiteSpace(selfId) && !string.IsNullOrWhiteSpace(senderId) && string.Equals(selfId, senderId, StringComparison.Ordinal))
            {
                try
                {
                    Singleton<GameDirector>.Instance?.PlayerUnitView?.PlayAnimation(anim);
                }
                catch
                {

                }
            }

            bool hasDamage = false;
            try
            {
                if (root.TryGetProperty("Actions", out JsonElement actionsEl) && actionsEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in actionsEl.EnumerateArray())
                    {
                        if (item.ValueKind != JsonValueKind.Object)
                        {
                            continue;
                        }

                        if (GetString(item, "Kind") == "Damage")
                        {
                            hasDamage = true;
                            break;
                        }
                    }
                }
            }
            catch
            {
                hasDamage = false;
            }

            if (!hasDamage || string.IsNullOrWhiteSpace(targetId))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(selfId) && string.Equals(selfId, targetId, StringComparison.Ordinal))
            {
                try
                {
                    Singleton<GameDirector>.Instance?.PlayerUnitView?.PlayAnimation("hit");
                }
                catch
                {

                }
                return;
            }

            if (OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView(targetId, out UnitView targetView))
            {
                targetView.PlayAnimation("hit");
            }
        }
        catch
        {

        }
    }

        private static string MapCardTypeToAnimation(string cardType)
    {
        return cardType switch
        {
            nameof(CardType.Attack) => "shoot1",
            nameof(CardType.Defense) => "defend",
            nameof(CardType.Skill) => "skill",
            nameof(CardType.Ability) => "spell",
            nameof(CardType.Tool) => "spell",
            _ => "spell"
        };
    }

    #endregion
}
