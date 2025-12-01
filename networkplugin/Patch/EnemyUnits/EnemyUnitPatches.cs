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
    /// <param name="_instance">敌方单位实例</param>
    /// <param name="hp">当前血量</param>
    /// <param name="maxHp">最大血量</param>
    /// <returns>返回false跳过原始方法执行</returns>
    [HarmonyPatch(typeof(EnemyUnit), "SetMaxHpInBattle")]
    [HarmonyPostfix]
    public static bool SetMaxHpInBattle(EnemyUnit _instance, int hp, int maxHp)
    {
        try
        {
            // 安全检查：确保网络管理器可用
            if (networkManager == null)
            {
                Plugin.Logger?.LogWarning("[EnemyUnitPatches] NetworkManager is null, skipping HP adjustment");
                return true; // 允许原始方法执行
            }

            // 获取当前玩家数量作为调整系数
            // TODO: 改为从config读取，支持自定义调整系数
            int playerCount = networkManager.GetPlayerCount();
            int hpCoefficient = Math.Max(1, playerCount); // 确保系数至少为1

            // 计算调整后的血量
            int modifiedHp = hp * hpCoefficient;
            int modifiedMaxHp = maxHp * hpCoefficient;

            // 使用Traverse调用基类的SetMaxHp方法
            // 这是一种反射技术，用于调用被Harmony补丁覆盖的原始方法
            Type baseType = typeof(Unit);

            Traverse.CreateWithType(baseType.FullName)
                    .Method("SetMaxHp", modifiedHp, modifiedMaxHp)
                    .GetValue(_instance);

            Plugin.Logger?.LogInfo($"[EnemyUnitPatches] Adjusted enemy HP: {maxHp} -> {modifiedMaxHp} (x{hpCoefficient} for {playerCount} players)");

            return false; // 跳过原始方法执行，因为我们已经手动调用了基类方法
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnemyUnitPatches] Error adjusting enemy HP: {ex.Message}");
            return true; // 出错时允许原始方法执行
        }
    }

    #endregion
}