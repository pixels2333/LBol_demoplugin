using System;
using HarmonyLib;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.EnemyUnits;

/// <summary>
/// 敌方单位补丁类
/// 负责调整敌方单位的属性以适应多人游戏模式
/// 主要包括血量调整、难度平衡等功能
/// </summary>
[HarmonyPatch]
public class EnemyUnitPatches
{
    #region 私有字段

    /// <summary>
    /// 服务提供者，用于获取依赖注入的服务
    /// </summary>
    private static IServiceProvider serviceProvider = ModService.ServiceProvider;

    /// <summary>
    /// 网络管理器，用于获取玩家数量等信息
    /// </summary>
    private static INetworkManager networkManager => serviceProvider?.GetRequiredService<INetworkManager>();

    #endregion

    #region 血量调整补丁

    /// <summary>
    /// 敌方单位血量设置补丁
    /// 在多人游戏中，根据当前玩家数量调整敌方单位的血量
    /// 这样可以平衡游戏难度，确保多人游戏时的挑战性
    /// </summary>
    /// <param name="__instance">敌方单位实例</param>
    /// <param name="hp">当前血量</param>
    /// <param name="maxHp">最大血量</param>
    /// <remarks>
    /// Harmony 注入实例参数必须命名为 __instance；如果写成 _instance，Harmony 会把它当作“原方法参数名”，
    /// 导致在 PatchAll 阶段报错：Parameter "_instance" not found。
    /// </remarks>
    [HarmonyPatch(typeof(EnemyUnit), "SetMaxHpInBattle")]
    [HarmonyPrefix]
    public static void SetMaxHpInBattle(EnemyUnit __instance, ref int hp, ref int maxHp)
    {
        try
        {
            // 安全检查：确保网络管理器可用
            if (networkManager == null)
            {
                Plugin.Logger?.LogWarning("[EnemyUnitPatches] NetworkManager is null, skipping HP adjustment");
                return; // 允许原始方法执行
            }

            // 获取当前玩家数量作为调整系数（并叠加配置中的倍率）
            int playerCount = networkManager.GetPlayerCount();
            float multiplier = Plugin.ConfigManager?.GetEnemyHpMultiplier(playerCount) ?? playerCount;
            multiplier = Math.Max(1.0f, multiplier);

            // 直接改写入参，让原方法用调整后的数值执行。
            int originalMaxHp = maxHp;
            hp = (int)Math.Round(hp * multiplier);
            maxHp = (int)Math.Round(maxHp * multiplier);
            if (hp < 1) hp = 1;
            if (maxHp < 1) maxHp = 1;

            Plugin.Logger?.LogInfo($"[EnemyUnitPatches] Adjusted enemy HP: {originalMaxHp} -> {maxHp} (x{multiplier:0.##} for {playerCount} players)");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemyUnitPatches] Error adjusting enemy HP: {ex.Message}");
            // 出错时保持入参不变，让原始方法照常执行。
        }
    }

    #endregion
}
