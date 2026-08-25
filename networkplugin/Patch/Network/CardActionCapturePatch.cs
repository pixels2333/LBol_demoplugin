using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Patch.UI;
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 卡牌与符卡动作捕获补丁：
/// 拦截本地出牌/用符卡时生成的实际 <see cref="BattleAction"/> 序列，
/// 生成包含所有弹幕、粒子特效、动画和音效的动作蓝图并广播给所有玩家。
/// </summary>
[HarmonyPatch]
public static class CardActionCapturePatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetClient()
        => ServiceProvider?.GetService<INetworkClient>();

    private static bool TryPrepareClient(out INetworkClient client, out bool isHost, out string selfPlayerId)
    {
        client = TryGetClient();
        if (client == null || !client.IsConnected)
        {
            isHost = false;
            selfPlayerId = null;
            return false;
        }

        isHost = NetworkIdentityTracker.GetSelfIsHost();
        selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
        return !string.IsNullOrWhiteSpace(selfPlayerId);
    }

    private static string ResolveSelfPlayerName()
    {
        string selfId = NetworkIdentityTracker.GetSelfPlayerId();
        return OtherPlayersOverlayPatch.ResolveDisplayName(selfId, null, isLocal: true) ?? "Player";
    }

    #region Card.GetActions 拦截与动作收集

    [HarmonyPatch(typeof(Card), "GetActions")]
    [HarmonyPostfix]
    public static void Card_GetActions_Postfix(
        Card __instance,
        UnitSelector selector,
        ManaGroup consumingMana,
        Interaction precondition,
        bool kicker,
        bool summoning,
        IList<DamageAction> damageActions,
        ref IEnumerable<BattleAction> __result)
    {
        try
        {
            if (__result == null)
            {
                return;
            }

            // 远程卡牌管道或未联网时不进行捕获
            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            if (!TryPrepareClient(out _, out _, out _))
            {
                return;
            }

            // 只捕获本地玩家拥有的卡牌
            if (__instance?.Battle?.Player == null)
            {
                return;
            }

            string cardId = __instance.Id;
            string cardName = __instance.Name;

            __result = WrapActionEnumerable(__result, cardId, cardName, isUs: false, selector);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardActionCapture] Card_GetActions_Postfix error: {ex.Message}");
        }
    }

    #endregion

    #region UltimateSkill.GetActions 拦截与动作收集

    [HarmonyPatch(typeof(UltimateSkill), "GetActions")]
    [HarmonyPostfix]
    public static void UltimateSkill_GetActions_Postfix(
        UltimateSkill __instance,
        UnitSelector selector,
        IList<DamageAction> damageActions,
        ref IEnumerable<BattleAction> __result)
    {
        try
        {
            if (__result == null)
            {
                return;
            }

            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            if (!TryPrepareClient(out _, out _, out _))
            {
                return;
            }

            string usId = __instance?.Id ?? "UnknownUs";
            string usName = __instance?.Name ?? __instance?.DebugName ?? "符卡";

            __result = WrapActionEnumerable(__result, usId, usName, isUs: true, selector);
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardActionCapture] UltimateSkill_GetActions_Postfix error: {ex.Message}");
        }
    }

    #endregion

    #region 动作流包装器与广播

    private static IEnumerable<BattleAction> WrapActionEnumerable(
        IEnumerable<BattleAction> original,
        string cardOrUsId,
        string cardOrUsName,
        bool isUs,
        UnitSelector selector)
    {
        if (original == null)
        {
            yield break;
        }

        List<BattleAction> capturedActions = new List<BattleAction>();
        IEnumerator<BattleAction> enumerator = null;

        try
        {
            enumerator = original.GetEnumerator();
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardActionCapture] GetEnumerator failed for {cardOrUsName}: {ex.Message}");
            yield break;
        }

        bool hasMore = true;
        while (hasMore)
        {
            BattleAction current = null;
            try
            {
                hasMore = enumerator.MoveNext();
                if (hasMore)
                {
                    current = enumerator.Current;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[CardActionCapture] MoveNext failed for {cardOrUsName}: {ex.Message}");
                hasMore = false;
            }

            if (hasMore && current != null)
            {
                capturedActions.Add(current);
                yield return current;
            }
        }

        try
        {
            enumerator?.Dispose();
        }
        catch
        {
            // ignored
        }

        // 当动作枚举结束，生成完整动作蓝图并广播
        SendCapturedActionsBroadcast(cardOrUsId, cardOrUsName, isUs, selector, capturedActions);
    }

    private static void SendCapturedActionsBroadcast(
        string cardOrUsId,
        string cardOrUsName,
        bool isUs,
        UnitSelector selector,
        List<BattleAction> actions)
    {
        try
        {
            if (!TryPrepareClient(out INetworkClient client, out bool isHost, out string selfPlayerId))
            {
                return;
            }

            object[] actionBlueprint = Array.Empty<object>();
            if (actions != null && actions.Count > 0)
            {
                actionBlueprint = RemoteCardUsePatch.BuildActionBlueprint(actions);
            }

            string eventType;
            if (isUs)
            {
                eventType = isHost
                    ? NetworkMessageTypes.BattlePlayerUsUsedBroadcast
                    : NetworkMessageTypes.BattlePlayerUsUsedReport;
            }
            else
            {
                eventType = isHost
                    ? NetworkMessageTypes.BattlePlayerCardUsedBroadcast
                    : NetworkMessageTypes.BattlePlayerCardUsedReport;
            }

            var payload = new
            {
                Timestamp = DateTime.Now.Ticks,
                PlayerId = selfPlayerId,
                PlayerName = ResolveSelfPlayerName(),
                IsHost = isHost,
                CardName = cardOrUsName,
                CardId = cardOrUsId,
                UsName = isUs ? cardOrUsName : null,
                IsUs = isUs,
                Actions = actionBlueprint,
            };

            client.SendGameEventData(eventType, payload);
            Plugin.Logger?.LogInfo($"[CardActionCapture] 已广播全特效出牌事件: {eventType} CardName={cardOrUsName}, ActionsCount={actionBlueprint.Length}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardActionCapture] SendCapturedActionsBroadcast error: {ex.Message}");
        }
    }

    #endregion
}
