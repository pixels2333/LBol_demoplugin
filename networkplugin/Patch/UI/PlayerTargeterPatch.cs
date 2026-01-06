using System;
using System.Collections.Generic;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Units;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 参照 Together in Spire 的 PlayerTargeter.java：
/// 在“单体目标选择”状态下，把其它玩家的角色实体也加入可指向目标列表，
/// 并临时开启其 SelectorCollider 以便被鼠标射线命中，从而显示 SelectingVisible/指向反馈。
/// </summary>
[HarmonyPatch]
public static class PlayerTargeterPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static bool IsConnected()
    {
        try
        {
            INetworkClient client = ServiceProvider?.GetService<INetworkClient>();
            return client?.IsConnected == true;
        }
        catch
        {
            return false;
        }
    }

    private static bool ShouldEnable(TargetType targetType)
        => IsConnected() && targetType == TargetType.SingleEnemy;

    private static TargetType GetTargetType(TargetSelector selector)
    {
        try
        {
            return Traverse.Create(selector).Field("_targetType").GetValue<TargetType>();
        }
        catch
        {
            return TargetType.Nobody;
        }
    }

    [HarmonyPatch(typeof(TargetSelector), "SetPotentialTargets")]
    [HarmonyPostfix]
    private static void TargetSelector_SetPotentialTargets_Postfix(TargetSelector __instance)
    {
        try
        {
            if (__instance == null)
            {
                return;
            }

            TargetType targetType = GetTargetType(__instance);
            if (!ShouldEnable(targetType))
            {
                return;
            }

            var potentialTargets = Traverse.Create(__instance).Field("_potentialTargets").GetValue<List<UnitView>>();
            if (potentialTargets == null)
            {
                return;
            }

            foreach (UnitView remote in OtherPlayersOverlayPatch.SnapshotRemoteCharacterUnitViews())
            {
                if (remote?.Unit == null || !remote.Unit.IsAlive)
                {
                    continue;
                }

                if (!potentialTargets.Contains(remote))
                {
                    potentialTargets.Add(remote);
                }
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(TargetSelector), nameof(TargetSelector.EnableSelector), typeof(HandCard))]
    [HarmonyPostfix]
    private static void TargetSelector_EnableSelector_Hand_Postfix(HandCard hand)
    {
        try
        {
            if (hand?.Card?.Config?.TargetType == null)
            {
                return;
            }

            if (ShouldEnable(hand.Card.Config.TargetType.Value))
            {
                OtherPlayersOverlayPatch.SetRemoteCharacterTargetingEnabled(true);
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(TargetSelector), nameof(TargetSelector.EnableSelector), typeof(UltimateSkill), typeof(UnityEngine.Vector3))]
    [HarmonyPostfix]
    private static void TargetSelector_EnableSelector_Us_Postfix(UltimateSkill us)
    {
        try
        {
            if (us == null)
            {
                return;
            }

            if (ShouldEnable(us.TargetType))
            {
                OtherPlayersOverlayPatch.SetRemoteCharacterTargetingEnabled(true);
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(TargetSelector), nameof(TargetSelector.EnableSelector), typeof(Doll), typeof(UnityEngine.Vector3))]
    [HarmonyPostfix]
    private static void TargetSelector_EnableSelector_Doll_Postfix(Doll doll)
    {
        try
        {
            if (doll == null)
            {
                return;
            }

            if (ShouldEnable(doll.TargetType))
            {
                OtherPlayersOverlayPatch.SetRemoteCharacterTargetingEnabled(true);
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(TargetSelector), nameof(TargetSelector.DisableSelector))]
    [HarmonyPrefix]
    private static void TargetSelector_DisableSelector_Prefix()
    {
        try
        {
            if (!IsConnected())
            {
                return;
            }

            OtherPlayersOverlayPatch.SetRemoteCharacterTargetingEnabled(false);
        }
        catch
        {
            // ignored
        }
    }
}
