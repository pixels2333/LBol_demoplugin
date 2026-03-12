using System;
using System.Collections.Generic;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Units;
using LBoL.Presentation;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private static readonly Dictionary<string, RemotePlayerProxyEnemy> _proxyTargets = new(StringComparer.Ordinal);

    private static RemotePlayerProxyEnemy GetOrCreateProxyTarget(string playerId, string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            return null;
        }

        if (_proxyTargets.TryGetValue(playerId, out RemotePlayerProxyEnemy existing) && existing != null)
        {
            existing.UpdateDisplayName(playerName);
            return existing;
        }

        RemotePlayerProxyEnemy created = new RemotePlayerProxyEnemy(playerId, playerName);
        _proxyTargets[playerId] = created;
        return created;
    }

    private static bool IsConnected()
        => ServiceProvider?.GetService<INetworkClient>()?.IsConnected == true;

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

    private static void SetPendingTarget(TargetSelector selector, EnemyUnit target)
    {
        if (selector == null)
        {
            return;
        }

        try
        {
            HandCard hand = Traverse.Create(selector).Field("_activeHand").GetValue<HandCard>();
            if (hand?.Card != null)
            {
                hand.Card.PendingTarget = target;
                return;
            }
        }
        catch
        {
            // 忽略反射失败，继续尝试其他目标来源。
        }

        try
        {
            UltimateSkill us = Traverse.Create(selector).Field("_activeUs").GetValue<UltimateSkill>();
            if (us != null)
            {
                us.PendingTarget = target;
                return;
            }
        }
        catch
        {
            // 忽略反射失败，继续尝试其他目标来源。
        }

        try
        {
            Doll doll = Traverse.Create(selector).Field("_activeDoll").GetValue<Doll>();
            doll?.PendingTarget = target;
        }
        catch
        {
            // 忽略反射失败，继续尝试其他目标来源。
        }
    }

    [HarmonyPatch(typeof(TargetSelector), "UpdateSingleEnemy")]
    [HarmonyPostfix]
    private static void TargetSelector_UpdateSingleEnemy_Postfix(TargetSelector __instance)
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

        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        Vector2 screenPosition = mouse.position.ReadValue();
        if (screenPosition == Vector2.zero)
        {
            return;
        }

        Ray ray = CameraController.MainCamera.ScreenPointToRay(screenPosition);
        bool selected = false;
        foreach (UnitView remote in OtherPlayersOverlayPatch.SnapshotRemoteCharacterUnitViews())
        {
            bool hit = !selected && remote.SelectorCollider != null &&
                remote.SelectorCollider.Raycast(ray, out _, float.PositiveInfinity);

            remote.SelectingVisible = hit;
            if (hit)
            {
                selected = true;
            }
        }

        if (selected && OtherPlayersOverlayPatch.TryGetPointedRemotePlayer(screenPosition, out string playerId, out string playerName))
        {
            RemotePlayerProxyEnemy proxy = GetOrCreateProxyTarget(playerId, playerName);
            if (proxy != null)
            {
                SetPendingTarget(__instance, proxy);
            }
        }
    }

    [HarmonyPatch(typeof(TargetSelector), "GetPointedEnemy")]
    [HarmonyPrefix]
    private static bool TargetSelector_GetPointedEnemy_Prefix(Vector2 screenPosition, ref EnemyUnit __result)
    {
        if (!IsConnected())
        {
            return true;
        }

        if (!OtherPlayersOverlayPatch.TryGetPointedRemotePlayer(screenPosition, out string playerId, out string playerName))
        {
            return true;
        }

        RemotePlayerProxyEnemy proxy = GetOrCreateProxyTarget(playerId, playerName);
        if (proxy == null)
        {
            return true;
        }

        __result = proxy;
        return false;
    }

    [HarmonyPatch(typeof(TargetSelector), nameof(TargetSelector.EnableSelector), typeof(HandCard))]
    [HarmonyPostfix]
    private static void TargetSelector_EnableSelector_Hand_Postfix(HandCard hand)
    {
        TargetType? targetType = hand?.Card?.Config?.TargetType;
        if (targetType == null)
        {
            return;
        }

        if (ShouldEnable(targetType.Value))
        {
            OtherPlayersOverlayPatch.SetRemoteCharacterTargetingEnabled(true);
        }
    }

    [HarmonyPatch(typeof(TargetSelector), nameof(TargetSelector.EnableSelector), typeof(UltimateSkill), typeof(Vector3))]
    [HarmonyPostfix]
    private static void TargetSelector_EnableSelector_Us_Postfix(UltimateSkill us)
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

    [HarmonyPatch(typeof(TargetSelector), nameof(TargetSelector.EnableSelector), typeof(Doll), typeof(Vector3))]
    [HarmonyPostfix]
    private static void TargetSelector_EnableSelector_Doll_Postfix(Doll doll)
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

    [HarmonyPatch(typeof(TargetSelector), nameof(TargetSelector.DisableSelector))]
    [HarmonyPrefix]
    private static void TargetSelector_DisableSelector_Prefix()
    {
        if (!IsConnected())
        {
            return;
        }

        OtherPlayersOverlayPatch.SetRemoteCharacterTargetingEnabled(false);
        foreach (UnitView remote in OtherPlayersOverlayPatch.SnapshotRemoteCharacterUnitViews())
        {
            remote.SelectingVisible = false;
        }
    }
}
