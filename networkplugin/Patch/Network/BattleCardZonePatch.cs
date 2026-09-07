using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

public static class BattleCardZoneSyncPatch
{
        private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

        private static INetworkClient TryGetNetworkClient()
        => ServiceProvider?.GetService<INetworkClient>();

        private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {

            INetworkClient networkClient = TryGetNetworkClient();
            if (networkClient == null || !networkClient.IsConnected)
            {

                return;
            }

            networkClient.SendGameEventData(eventType, eventData);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[BattleCardZoneSync] Error sending game event {eventType}: {ex.Message}");
        }
    }

        private static object BuildCardSnapshot(Card card)
    {

        string costText = null;
        try
        {
            costText = card?.Cost.ToString();
        }
        catch
        {
            costText = null;
        }

        return new
        {
            CardId = card?.Id ?? "null",
            InstanceId = card?.InstanceId ?? -1,
            CardName = card?.Name ?? "null",
            CardType = card?.GetType().Name ?? "null",
            IsUpgraded = card?.IsUpgraded ?? false,
            CostText = costText,
            Zone = card?.Zone.ToString() ?? "Unknown",
            IsEthereal = card?.IsEthereal ?? false,
            IsAutoExile = card?.IsAutoExile ?? false,
        };
    }

        private static bool ShouldSync(BattleController battle)
        => SendSyncHelper.ShouldSyncBattle(battle);

    #region MoveCard 补丁

        [HarmonyPatch(typeof(BattleController), "MoveCard")]
    private static class MoveCardPatch
    {
                private struct MoveState
        {
                        public CardZone FromZone;

                        public CardZone ToZone;

                        public object CardSnapshotBefore;
        }

                [HarmonyPrefix]
        private static void Prefix(BattleController __instance, Card card, CardZone dstZone, out MoveState __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = new MoveState
                {
                    FromZone = card.Zone,
                    ToZone = dstZone,
                    CardSnapshotBefore = BuildCardSnapshot(card)
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in MoveCard Prefix: {ex.Message}");
            }
        }

                [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, CardZone dstZone, CancelCause __result, MoveState __state)
        {
            try
            {

                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "ZoneChanged",
                    FromZone = __state.FromZone.ToString(),
                    ToZone = dstZone.ToString(),
                    CancelCause = __result.ToString(),
                    CardBefore = __state.CardSnapshotBefore,
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in MoveCard Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region MoveCardToDrawZone 补丁

        [HarmonyPatch(typeof(BattleController), "MoveCardToDrawZone")]
    private static class MoveCardToDrawZonePatch
    {
                private struct MoveToDrawState
        {
                        public CardZone FromZone;

                        public DrawZoneTarget Target;

                        public object CardSnapshotBefore;
        }

                [HarmonyPrefix]
        private static void Prefix(BattleController __instance, Card card, DrawZoneTarget target, out MoveToDrawState __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = new MoveToDrawState
                {
                    FromZone = card.Zone,
                    Target = target,
                    CardSnapshotBefore = BuildCardSnapshot(card)
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in MoveCardToDrawZone Prefix: {ex.Message}");
            }
        }

                [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, DrawZoneTarget target, CancelCause __result, MoveToDrawState __state)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "ZoneChanged",
                    FromZone = __state.FromZone.ToString(),
                    ToZone = CardZone.Draw.ToString(),
                    DrawTarget = __state.Target.ToString(),
                    CancelCause = __result.ToString(),
                    CardBefore = __state.CardSnapshotBefore,
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in MoveCardToDrawZone Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region AddCardToDrawZone 补丁

        [HarmonyPatch(typeof(BattleController), "AddCardToDrawZone")]
    private static class AddCardToDrawZonePatch
    {
                [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, DrawZoneTarget target, CancelCause __result)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardAdded",
                    ToZone = CardZone.Draw.ToString(),
                    DrawTarget = target.ToString(),
                    CancelCause = __result.ToString(),
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in AddCardToDrawZone Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region AddCardToHand 补丁

        [HarmonyPatch(typeof(BattleController), "AddCardToHand")]
    private static class AddCardToHandPatch
    {
                [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, CancelCause __result)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardAdded",
                    ToZone = CardZone.Hand.ToString(),
                    CancelCause = __result.ToString(),
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in AddCardToHand Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region AddCardToDiscard 补丁

        [HarmonyPatch(typeof(BattleController), "AddCardToDiscard")]
    private static class AddCardToDiscardPatch
    {
                [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, CancelCause __result)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardAdded",
                    ToZone = CardZone.Discard.ToString(),
                    CancelCause = __result.ToString(),
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in AddCardToDiscard Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region AddCardToExile 补丁

        [HarmonyPatch(typeof(BattleController), "AddCardToExile")]
    private static class AddCardToExilePatch
    {
                [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, CancelCause __result)
        {
            try
            {
                if (!ShouldSync(__instance) || __result != CancelCause.None)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardAdded",
                    ToZone = CardZone.Exile.ToString(),
                    CancelCause = __result.ToString(),
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in AddCardToExile Postfix: {ex.Message}");
            }
        }
    }

    #endregion

    #region RemoveCard 补丁

        [HarmonyPatch(typeof(BattleController), "RemoveCard")]
    private static class RemoveCardPatch
    {
                private struct RemoveState
        {
                        public CardZone FromZone;

                        public object CardSnapshotBefore;
        }

                [HarmonyPrefix]
        private static void Prefix(BattleController __instance, Card card, out RemoveState __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = new RemoveState
                {
                    FromZone = card.Zone,
                    CardSnapshotBefore = BuildCardSnapshot(card)
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in RemoveCard Prefix: {ex.Message}");
            }
        }

                [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, RemoveState __state)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.CardStateChanged,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    ChangeType = "CardRemovedFromBattle",
                    FromZone = __state.FromZone.ToString(),
                    ToZone = CardZone.None.ToString(),
                    CardBefore = __state.CardSnapshotBefore,
                    CardAfter = BuildCardSnapshot(card)
                };

                SendGameEvent(NetworkMessageTypes.CardStateChanged, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[BattleCardZoneSync] Error in RemoveCard Postfix: {ex.Message}");
            }
        }
    }

    #endregion
}
