using System;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Battle;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

public static class EnergySyncPatch
{
    private static INetworkClient TryGetNetworkClient()
        => SendSyncHelper.TryGetClient();

    private static bool ShouldSync(BattleController battle)
        => SendSyncHelper.ShouldSyncBattle(battle);

    private static void SendGameEvent(string eventType, object eventData)
    {
        var client = SendSyncHelper.TryGetClient();
        if (client == null) return;
        client.SendGameEventData(eventType, eventData);
    }

    private static object SnapshotMana(ManaGroup mana)
        => new
        {
            Red = mana.Red,
            Blue = mana.Blue,
            Green = mana.Green,
            White = mana.White,
            Colorless = mana.Colorless,
            Philosophy = mana.Philosophy,
            Any = mana.Any,
            Hybrid = mana.Hybrid,
            HybridColor = mana.HybridColor.ToString(),
            Total = mana.Total
        };

    private static object BuildManaChangePayload(BattleController battle, string changeType, ManaGroup before, ManaGroup after, object detail = null)
        => new
        {
            Timestamp = DateTime.Now.Ticks,
            EventType = changeType,
            PlayerId = GameStateUtils.GetCurrentPlayerId(),
            BattleId = battle.GetHashCode(),
            ManaBefore = SnapshotMana(before),
            ManaAfter = SnapshotMana(after),
            Detail = detail
        };

    private static object BuildTurnManaPayload(BattleController battle, ManaGroup before, ManaGroup after, ManaGroup requested, ManaGroup applied)
        => new
        {
            Timestamp = DateTime.Now.Ticks,
            EventType = NetworkMessageTypes.TurnManaCalculated,
            PlayerId = GameStateUtils.GetCurrentPlayerId(),
            ExtraTurnManaBefore = SnapshotMana(before),
            ExtraTurnManaAfter = SnapshotMana(after),
            Requested = SnapshotMana(requested),
            Applied = SnapshotMana(applied),
            TurnManaNow = SnapshotMana(battle.TurnMana),
            LockedTurnManaNow = SnapshotMana(battle.LockedTurnMana),
            BaseTurnManaNow = SnapshotMana(battle.BaseTurnMana)
        };

    [HarmonyPatch(typeof(BattleController), "GainMana")]
    private static class GainManaPatch
    {
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, ManaGroup group, out ManaGroup __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = __instance.BattleMana;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in GainMana Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, ManaGroup group, ManaGroup __result, ManaGroup __state)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                var after = __instance.BattleMana;
                if (after == __state)
                {
                    return;
                }

                var detail = new
                {
                    Requested = SnapshotMana(group),
                    Applied = SnapshotMana(__result)
                };

                SendGameEvent(NetworkMessageTypes.ManaRegain, BuildManaChangePayload(__instance, NetworkMessageTypes.ManaRegain, __state, after, detail));
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in GainMana Postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "LoseMana")]
    private static class LoseManaPatch
    {
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, ManaGroup group, out ManaGroup __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = __instance.BattleMana;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in LoseMana Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, ManaGroup group, ManaGroup __result, ManaGroup __state)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                var after = __instance.BattleMana;
                if (after == __state)
                {
                    return;
                }

                var detail = new
                {
                    Requested = SnapshotMana(group),
                    Applied = SnapshotMana(__result)
                };

                SendGameEvent(NetworkMessageTypes.ManaConsumeCompleted, BuildManaChangePayload(__instance, NetworkMessageTypes.ManaConsumeCompleted, __state, after, detail));
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in LoseMana Postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "ConsumeMana")]
    private static class ConsumeManaPatch
    {
        private struct ConsumeState
        {
            public ManaGroup Before;
            public ManaGroup Cost;
        }

        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, ManaGroup group, out ConsumeState __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = new ConsumeState
                {
                    Before = __instance.BattleMana,
                    Cost = group
                };

                var startedDetail = new { Consuming = SnapshotMana(group) };
                SendGameEvent(NetworkMessageTypes.ManaConsumeStarted, BuildManaChangePayload(__instance, NetworkMessageTypes.ManaConsumeStarted, __state.Before, __state.Before, startedDetail));
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in ConsumeMana Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, ManaGroup group, ConsumeState __state)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                var after = __instance.BattleMana;
                if (after == __state.Before)
                {
                    return;
                }

                var completedDetail = new { Consumed = SnapshotMana(__state.Cost) };
                SendGameEvent(NetworkMessageTypes.ManaConsumeCompleted, BuildManaChangePayload(__instance, NetworkMessageTypes.ManaConsumeCompleted, __state.Before, after, completedDetail));
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in ConsumeMana Postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "ConvertMana")]
    private static class ConvertManaPatch
    {
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, ManaGroup input, ManaGroup output, bool allowPartial, out ManaGroup __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = __instance.BattleMana;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in ConvertMana Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, ManaGroup input, ManaGroup output, bool allowPartial, bool __result, ManaGroup resultInput, ManaGroup resultOutput, ManaGroup __state)
        {
            try
            {
                if (!ShouldSync(__instance) || !__result)
                {
                    return;
                }

                var after = __instance.BattleMana;
                if (after == __state)
                {
                    return;
                }

                var detail = new
                {
                    AllowPartial = allowPartial,
                    InputRequested = SnapshotMana(input),
                    OutputRequested = SnapshotMana(output),
                    InputApplied = SnapshotMana(resultInput),
                    OutputApplied = SnapshotMana(resultOutput),
                };

                SendGameEvent(NetworkMessageTypes.ManaConsumeCompleted, BuildManaChangePayload(__instance, "ManaConverted", __state, after, detail));
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in ConvertMana Postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "GainTurnMana")]
    private static class GainTurnManaPatch
    {
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, ManaGroup group, out ManaGroup __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = __instance.ExtraTurnMana;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in GainTurnMana Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, ManaGroup group, ManaGroup __result, ManaGroup __state)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                var after = __instance.ExtraTurnMana;
                if (after == __state)
                {
                    return;
                }

                object payload = BuildTurnManaPayload(__instance, __state, after, group, __result);

                SendGameEvent(NetworkMessageTypes.TurnManaCalculated, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in GainTurnMana Postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "LoseTurnMana")]
    private static class LoseTurnManaPatch
    {
        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, ManaGroup group, out ManaGroup __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                __state = __instance.ExtraTurnMana;
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in LoseTurnMana Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, ManaGroup group, ManaGroup __result, ManaGroup __state)
        {
            try
            {
                if (!ShouldSync(__instance))
                {
                    return;
                }

                var after = __instance.ExtraTurnMana;
                if (after == __state)
                {
                    return;
                }

                object payload = BuildTurnManaPayload(__instance, __state, after, group, __result);

                SendGameEvent(NetworkMessageTypes.TurnManaCalculated, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in LoseTurnMana Postfix: {ex.Message}");
            }
        }
    }
}
