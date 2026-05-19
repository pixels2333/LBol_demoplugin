using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Configuration;
using NetworkPlugin.Network.Services;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.UI;

/// <summary>
/// 仅用于调试：启用后，把本地玩家的战斗动画镜像到虚拟远程玩家（PlayerId=aidefault）。
/// 这样可以在没有真实网络对端时验证“远程玩家”渲染路径。
/// </summary>
[HarmonyPatch]
internal static class AiDefaultMimicLocalAnimationPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    [ThreadStatic]
    private static bool _isMirroring;

    private static bool IsEnabled()
    {
        var cfg = ServiceProvider?.GetService<ConfigManager>();
        return cfg?.DebugVirtualPlayerAiDefault?.Value == true;
    }

    private static bool IsNetworkConnected()
        => ServiceProvider?.GetService<INetworkClient>()?.IsConnected == true;

    [HarmonyPatch(typeof(UnitView), "PlayAnimation", typeof(string))]
    [HarmonyPostfix]
    private static void UnitView_PlayAnimation_Postfix(UnitView __instance, string animationName)
    {
        if (_isMirroring)
        {
            return;
        }

        if (!IsEnabled())
        {
            return;
        }

        // 避免影响真实联机对局。
        if (IsNetworkConnected())
        {
            return;
        }

        UnitView local = Singleton<GameDirector>.Instance?.PlayerUnitView;
        if (local == null || __instance == null)
        {
            return;
        }

        // 避免调试角色看起来像是承受了双倍受击动画。
        if (string.Equals(animationName, "hit", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 只镜像本地玩家自己的 UnitView 动画。
        if (!ReferenceEquals(__instance, local))
        {
            return;
        }

        if (!OtherPlayersOverlayPatch.TryGetRemoteCharacterUnitView("aidefault", out UnitView remote))
        {
            return;
        }

        _isMirroring = true;
        try
        {
            remote.PlayAnimation(animationName);
        }
        finally
        {
            _isMirroring = false;
        }
    }
}
