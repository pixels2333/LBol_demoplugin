using System;
using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 参照 Together in Spire: UnlockEverythingForMPPatch.java
///
/// 目标：联机时把“解锁相关判定”视为全部已解锁，避免因各玩家本地解锁进度不同导致的内容差异。
/// - <see cref="GameMaster.CurrentProfileLevel"/>：用于角色/谜题/博物馆等 UI 与解锁判定
/// - <see cref="GameRunStartupParameters.UnlockLevel"/>：用于开局可掉落/可抽取卡牌与展品池等内容生成
///
/// 注意：仅在 <see cref="INetworkClient.IsConnected"/> == true 时生效，不影响单机。
/// </summary>
[HarmonyPatch]
public static class UnlockEverythingForMPPatch
{
    private static IServiceProvider ServiceProvider => ModService.ServiceProvider;

    private static INetworkClient TryGetNetworkClient()
    {
        try
        {
            return ServiceProvider?.GetService<INetworkClient>();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsMultiplayerConnected()
        => TryGetNetworkClient()?.IsConnected == true;

    /// <summary>
    /// 联机时将档案等级视为最大等级，等价于 StS 的 treatEverythingAsUnlocked() == true 的效果。
    /// </summary>
    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.CurrentProfileLevel), MethodType.Getter)]
    private static class GameMaster_CurrentProfileLevel_Getter_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref int __result)
        {
            if (!IsMultiplayerConnected())
                return true;

            __result = ExpHelper.MaxLevel;
            return false;
        }
    }

    /// <summary>
    /// 联机时将开局 UnlockLevel 视为最大等级，确保卡牌/展品池等生成逻辑不受本地档案进度影响。
    /// </summary>
    [HarmonyPatch(typeof(GameRunStartupParameters), nameof(GameRunStartupParameters.UnlockLevel), MethodType.Getter)]
    private static class GameRunStartupParameters_UnlockLevel_Getter_Patch
    {
        [HarmonyPrefix]
        private static bool Prefix(ref int __result)
        {
            if (!IsMultiplayerConnected())
                return true;

            __result = ExpHelper.MaxLevel;
            return false;
        }
    }
}

