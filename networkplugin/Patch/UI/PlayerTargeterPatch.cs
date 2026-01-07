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

        var created = new RemotePlayerProxyEnemy(playerId, playerName);
        _proxyTargets[playerId] = created;
        return created;
    }

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
            // ignored
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
            // ignored
        }

        try
        {
            Doll doll = Traverse.Create(selector).Field("_activeDoll").GetValue<Doll>();
            if (doll != null)
            {
                doll.PendingTarget = target;
            }
        }
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(TargetSelector), "UpdateSingleEnemy")]
    [HarmonyPostfix]
    private static void TargetSelector_UpdateSingleEnemy_Postfix(TargetSelector __instance)
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
                if (remote == null)
                {
                    continue;
                }

                bool hit = false;
                try
                {
                    if (!selected && remote.SelectorCollider != null)
                    {
                        hit = remote.SelectorCollider.Raycast(ray, out _, float.PositiveInfinity);
                    }
                }
                catch
                {
                    hit = false;
                }

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
        catch
        {
            // ignored
        }
    }

    [HarmonyPatch(typeof(TargetSelector), "GetPointedEnemy")]
    [HarmonyPrefix]
    private static bool TargetSelector_GetPointedEnemy_Prefix(Vector2 screenPosition, ref EnemyUnit __result)
    {
        try
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
        catch
        {
            return true;
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
            foreach (UnitView remote in OtherPlayersOverlayPatch.SnapshotRemoteCharacterUnitViews())
            {
                if (remote != null)
                {
                    remote.SelectingVisible = false;
                }
            }
        }
        catch
        {
            // ignored
        }
    }
}
