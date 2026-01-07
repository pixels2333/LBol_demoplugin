using System;
using System.Collections.Generic;
using HarmonyLib;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 心境（Mood）切换/维持特效渲染重定向补丁（参照 Together in Spire: StanceSwitchRenderPatches.java）。
///
/// LBoL 的心境通过 StatusEffect.UnitEffectName 触发 UnitView.TryPlayEffectLoop/SendEffectMessage/EndEffectLoop，
/// 默认只会作用在本地 PlayerUnitView 上。
///
/// 在联机模式下，如果需要把心境循环特效渲染到“远程玩家的 UnitView”，可以在调用前设置 <see cref="RenderOn"/>，
/// 使上述 API 调用被重定向到指定 UnitView。
/// </summary>
[HarmonyPatch]
public static class MoodSwitchRenderPatches
{
    /// <summary>
    /// 指定本次心境相关特效应该渲染到哪个 UnitView 上。
    /// 用法建议：在触发心境效果前设置；完成后立刻恢复为 null（try/finally）。
    /// </summary>
    public static UnitView RenderOn;

    private static INetworkClient NetworkClient => ModService.ServiceProvider?.GetService<INetworkClient>();

    private static readonly HashSet<string> MoodEffectNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Koishi moods
        "ChaowoLoop", // MoodPeace
        "BenwoLoop",  // MoodPassion
        "DunwuLoop",  // MoodEpiphany
    };

    private static bool ShouldRedirect(string effectName)
    {
        return !string.IsNullOrWhiteSpace(effectName) && MoodEffectNames.Contains(effectName);
    }

    private static bool IsConnected()
    {
        try
        {
            return NetworkClient?.IsConnected == true;
        }
        catch
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(UnitView), nameof(UnitView.TryPlayEffectLoop))]
    [HarmonyPrefix]
    private static bool UnitView_TryPlayEffectLoop_Prefix(UnitView __instance, string effectName, ref bool __result)
    {
        if (!IsConnected() || RenderOn == null || ReferenceEquals(__instance, RenderOn) || !ShouldRedirect(effectName))
        {
            return true;
        }

        __result = RenderOn.TryPlayEffectLoop(effectName);
        return false;
    }

    [HarmonyPatch(typeof(UnitView), nameof(UnitView.SendEffectMessage))]
    [HarmonyPrefix]
    private static bool UnitView_SendEffectMessage_Prefix(UnitView __instance, string effectName, string message, object args)
    {
        if (!IsConnected() || RenderOn == null || ReferenceEquals(__instance, RenderOn) || !ShouldRedirect(effectName))
        {
            return true;
        }

        RenderOn.SendEffectMessage(effectName, message, args);
        return false;
    }

    [HarmonyPatch(typeof(UnitView), nameof(UnitView.EndEffectLoop))]
    [HarmonyPrefix]
    private static bool UnitView_EndEffectLoop_Prefix(UnitView __instance, string effectName, bool instant)
    {
        if (!IsConnected() || RenderOn == null || ReferenceEquals(__instance, RenderOn) || !ShouldRedirect(effectName))
        {
            return true;
        }

        RenderOn.EndEffectLoop(effectName, instant);
        return false;
    }
}

