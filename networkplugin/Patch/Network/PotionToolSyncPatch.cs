using System;
using System.Linq;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 药水/道具（Tool 卡）同步补丁：
/// LBoL 中“药水”更接近 Tool 卡（CardType.Tool + DeckCounter），其获取/移除发生在 GameRunController 的牌组增删，
/// 其使用发生在战斗中 UseCardAction 流程里并最终调用 BattleController.RecordCardUsage。
/// </summary>
public static class PotionToolSyncPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool ShouldSync(BattleController battle)
    {
        if (battle == null)
        {
            return false;
        }

        return battle.Player != null && battle.Player == GameStateUtils.GetCurrentPlayer();
    }

    private static void SendGameEvent(string eventType, object eventData)
    {
        try
        {
            var networkClient = ServiceProvider?.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            networkClient.SendGameEventData(eventType, eventData);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[PotionToolSync] Error sending game event {eventType}: {ex.Message}");
        }
    }

    private static object SnapshotToolCard(Card card)
        => new
        {
            CardId = card?.Id ?? "null",
            InstanceId = card?.InstanceId ?? -1,
            CardName = card?.Name ?? "null",
            CardType = card?.GetType().Name ?? "null",
            DeckCounter = card?.DeckCounter,
            IsUpgraded = card?.IsUpgraded ?? false
        };

    [HarmonyPatch(typeof(LBoL.Core.GameRunController), nameof(LBoL.Core.GameRunController.AddDeckCards))]
    private static class AddDeckCardsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(LBoL.Core.GameRunController __instance, System.Collections.Generic.IEnumerable<Card> cards, bool triggerVisual, LBoL.Core.VisualSourceData sourceData)
        {
            try
            {
                if (TradeSyncPatch.IsApplyingTrade)
                {
                    return;
                }

                var toolCards = cards?.Where(c => c != null && c.CardType == CardType.Tool).ToArray();
                if (toolCards == null || toolCards.Length == 0)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.OnPotionObtained,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    Tools = toolCards.Select(SnapshotToolCard).ToArray(),
                    Source = sourceData?.ToString() ?? "Unknown"
                };

                SendGameEvent(NetworkMessageTypes.OnPotionObtained, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionToolSync] Error in AddDeckCards Postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(LBoL.Core.GameRunController), nameof(LBoL.Core.GameRunController.RemoveDeckCards))]
    private static class RemoveDeckCardsPatch
    {
        [HarmonyPrefix]
        private static void Prefix(System.Collections.Generic.IEnumerable<Card> cards, out Card[] __state)
        {
            __state = Array.Empty<Card>();
            try
            {
                __state = cards?.Where(c => c != null && c.CardType == CardType.Tool).ToArray() ?? Array.Empty<Card>();
            }
            catch
            {
                __state = Array.Empty<Card>();
            }
        }

        [HarmonyPostfix]
        private static void Postfix(LBoL.Core.GameRunController __instance, Card[] __state, bool triggerVisual)
        {
            try
            {
                if (TradeSyncPatch.IsApplyingTrade)
                {
                    return;
                }

                if (__state == null || __state.Length == 0)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.OnPotionDiscarded,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    Tools = __state.Select(SnapshotToolCard).ToArray(),
                    TriggerVisual = triggerVisual
                };

                SendGameEvent(NetworkMessageTypes.OnPotionDiscarded, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionToolSync] Error in RemoveDeckCards Postfix: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(BattleController), "RecordCardUsage")]
    private static class RecordCardUsagePatch
    {
        private struct ToolUseState
        {
            public int? DeckCounterBefore;
            public bool IsTool;
            public object ToolSnapshotBefore;
        }

        [HarmonyPrefix]
        private static void Prefix(BattleController __instance, Card card, out ToolUseState __state)
        {
            __state = default;
            try
            {
                if (!ShouldSync(__instance) || card == null)
                {
                    return;
                }

                __state = new ToolUseState
                {
                    IsTool = card.CardType == CardType.Tool,
                    DeckCounterBefore = card.DeckCounter,
                    ToolSnapshotBefore = SnapshotToolCard(card)
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionToolSync] Error in RecordCardUsage Prefix: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        private static void Postfix(BattleController __instance, Card card, ToolUseState __state)
        {
            try
            {
                if (!ShouldSync(__instance) || card == null || !__state.IsTool)
                {
                    return;
                }

                var payload = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    EventType = NetworkMessageTypes.OnPotionUsed,
                    PlayerId = GameStateUtils.GetCurrentPlayerId(),
                    BattleId = __instance.GetHashCode(),
                    ToolBefore = __state.ToolSnapshotBefore,
                    ToolAfter = SnapshotToolCard(card),
                    DeckCounterBefore = __state.DeckCounterBefore,
                    DeckCounterAfter = card.DeckCounter
                };

                SendGameEvent(NetworkMessageTypes.OnPotionUsed, payload);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[PotionToolSync] Error in RecordCardUsage Postfix: {ex.Message}");
            }
        }
    }
}
