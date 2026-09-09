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

        NetworkIdentityTracker.EnsureSubscribed(client);
        isHost = NetworkIdentityTracker.GetSelfIsHost();
        selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
        if (string.IsNullOrWhiteSpace(selfPlayerId))
        {
            selfPlayerId = client.GetSelf()?.playerId;
        }
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

            if (RemoteCardUsePatch.IsInRemoteCardPipeline)
            {
                return;
            }

            if (!TryPrepareClient(out _, out _, out _))
            {
                return;
            }

            if (__instance?.Battle?.Player == null)
            {
                return;
            }

            string cardId = __instance.Id;
            string cardName = __instance.Name;

            var actionList = new List<BattleAction>();
            foreach (var act in __result)
            {
                if (act != null)
                {
                    actionList.Add(act);
                }
            }

            SendCapturedActionsBroadcast(cardId, cardName, isUs: false, selector, actionList);
            __result = actionList;
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

            var actionList = new List<BattleAction>();
            foreach (var act in __result)
            {
                if (act != null)
                {
                    actionList.Add(act);
                }
            }

            SendCapturedActionsBroadcast(usId, usName, isUs: true, selector, actionList);
            __result = actionList;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[CardActionCapture] UltimateSkill_GetActions_Postfix error: {ex.Message}");
        }
    }

    #endregion

    #region 动作蓝图广播

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

            string eventType = isUs
                ? NetworkMessageTypes.BattlePlayerUsUsedBroadcast
                : NetworkMessageTypes.BattlePlayerCardUsedBroadcast;

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
