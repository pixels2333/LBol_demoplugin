using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch;

/// <summary>
/// 玩家死亡处理补丁类
/// 使用Harmony框架拦截和修改LBoL游戏的玩家死亡相关逻辑
/// 实现联机模式下的假死（Fake Death）机制和死亡状态同步
/// </summary>
/// <remarks>
/// 这个类包含以下核心功能：
/// 1. 玩家假死处理 - 在联机模式下，玩家"死亡"时不会真正结束游戏，而是进入假死状态
/// 2. 死亡状态同步 - 将玩家的死亡状态同步给其他网络玩家
/// 3. 复活逻辑 - 处理玩家的复活机制（由Gap复活或其他方式）
/// 4. 死亡相关行为禁用 - 在假死状态下禁用玩家的某些游戏行为（抽牌、出牌等）
///
/// </remarks>
[HarmonyPatch]
public class DeathPatches
{
    /// <summary>
    /// 控制是否允许玩家真正死亡的标志
    /// 当为 false 时，玩家死亡会进入假死状态；为 true 时才能真正结束游戏
    /// 仅在所有玩家都死亡或游戏强制结束时设置为 true
    /// </summary>
    public static bool AllowRealDeath = false;

    /// <summary>
    /// 服务提供者访问器，用于获取依赖注入的组件
    /// 通过模块服务获取网络客户端和其他服务
    /// </summary>
    private static IServiceProvider ServiceProvider = ModService.ServiceProvider;

    /// <summary>
    /// 网络客户端属性，用于发送网络请求
    /// 采用延迟加载模式，避免初始化时的依赖问题
    /// </summary>
    private static INetworkClient NetworkClient => ServiceProvider?.GetRequiredService<INetworkClient>();

    #region 核心逻辑方法

    /// <summary>
    /// 检查玩家是否处于假死状态（在联机模式下）
    /// </summary>
    /// <param name="player">要检查的玩家单位</param>
    /// <returns>
    /// 在联机模式下，如果玩家已死且不允许真正死亡则返回 true；
    /// 否则返回 false（包括单人游戏模式）
    /// </returns>
    /// <remarks>
    /// 假死（Fake Death）是联机模式中的特殊状态：
    /// - 玩家生命值变为 0，标记为死亡（IsDead = true）
    /// - 但游戏不会立即结束，其他玩家继续游戏
    /// - 玩家可以通过复活机制（如Gap复活）恢复
    /// </remarks>
    private static bool IsPlayerInFakeDeath(PlayerUnit player)
    {
        // 单人游戏不使用假死机制
        if (NetworkClient == null || !NetworkClient.IsConnected)
        {
            return false;
        }

        // 联机模式：玩家已死且不允许真正死亡 = 假死状态
        return player != null && player.IsDead && !AllowRealDeath;
    }

    /// <summary>
    /// 处理玩家假死逻辑
    /// 当玩家生命值变为 0 时，恢复生命值到 1
    /// </summary>
    /// <param name="player">要处理的玩家单位</param>
    /// <remarks>
    /// 这个方法模拟了"假死"后的复苏状态：
    /// - 通过反射调用内部 Heal 方法将生命值恢复到 1（防止真正死亡）
    /// - 同步网络状态告知其他玩家该玩家仍存活
    /// </remarks>
    private static void HandleFakeDeath(PlayerUnit player)
    {
        if (player == null) return;

        try
        {
            // 使用反射调用内部的 Heal 方法
            // 如果当前 HP 为 0 或负数，计算需要恢复的量
            int hpToRecover = Math.Max(1, 1 - player.Hp);
            if (hpToRecover > 0)
            {
                var traverse = Traverse.Create(player);
                traverse.Method("Heal", hpToRecover).GetValue();
            }

            // 同步网络状态
            SyncPlayerDeathStatus(player, true);

            Plugin.Logger?.LogInfo($"[DeathPatch] Player {player.Id} entered fake death state (Hp: {player.Hp})");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in HandleFakeDeath: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 处理玩家复活逻辑
    /// 恢复玩家生命值，更新其他玩家状态
    /// </summary>
    /// <param name="player">要复活的玩家单位</param>
    /// <param name="resurrectionHp">复活后的生命值，如果为 null 则设置为最大生命值的 50%</param>
    /// <remarks>
    /// 复活（Resurrection）机制用于恢复假死的玩家：
    /// - 通常由特殊事件（如 Gap 的复活能力）触发
    /// - 恢复玩家生命值（如果未指定则为最大 HP 的 50%）
    /// - 通知其他玩家该玩家已复活
    /// - 更新游戏UI和游戏流程
    /// </remarks>
    private static void HandleResurrection(PlayerUnit player, int? resurrectionHp = null)
    {
        if (player == null) return;

        try
        {
            // 计算复活生命值
            int finalHp = resurrectionHp ?? (player.MaxHp / 2);
            if (finalHp <= 0) finalHp = 1; // 确保至少有 1 点生命值

            // 计算需要恢复的生命值
            int hpToRecover = finalHp - player.Hp;
            if (hpToRecover > 0)
            {
                var traverse = Traverse.Create(player);
                traverse.Method("Heal", hpToRecover).GetValue();
            }

            // 同步网络状态
            SyncPlayerResurrectionStatus(player, finalHp);

            Plugin.Logger?.LogInfo($"[DeathPatch] Player {player.Id} resurrected with Hp: {player.Hp}/{player.MaxHp}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in HandleResurrection: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 同步玩家的死亡状态到网络
    /// </summary>
    /// <param name="player">玩家单位</param>
    /// <param name="isFakeDeath">是否是假死</param>
    private static void SyncPlayerDeathStatus(PlayerUnit player, bool isFakeDeath)
    {
        if (NetworkClient == null || !NetworkClient.IsConnected) return;

        try
        {
            var deathData = new
            {
                PlayerId = player.Id,
                IsFakeDeath = isFakeDeath,
                Hp = player.Hp,
                MaxHp = player.MaxHp,
                Status = player.Status.ToString(),
                Timestamp = DateTime.Now.Ticks
            };

            string json = JsonSerializer.Serialize(deathData);
            NetworkClient.SendRequest("OnPlayerDeathStatusChanged", json);

            Plugin.Logger?.LogDebug($"[DeathPatch] Death status synced - IsFakeDeath: {isFakeDeath}, Hp: {player.Hp}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error syncing death status: {ex.Message}");
        }
    }

    /// <summary>
    /// 同步玩家的复活状态到网络
    /// </summary>
    /// <param name="player">玩家单位</param>
    /// <param name="resurrectionHp">复活后的生命值</param>
    private static void SyncPlayerResurrectionStatus(PlayerUnit player, int resurrectionHp)
    {
        if (NetworkClient == null || !NetworkClient.IsConnected) return;

        try
        {
            var resurrectionData = new
            {
                PlayerId = player.Id,
                ResurrectionHp = resurrectionHp,
                MaxHp = player.MaxHp,
                Status = player.Status.ToString(),
                Timestamp = DateTime.Now.Ticks
            };

            string json = JsonSerializer.Serialize(resurrectionData);
            NetworkClient.SendRequest("OnPlayerResurrected", json);

            Plugin.Logger?.LogDebug($"[DeathPatch] Resurrection status synced - Hp: {resurrectionHp}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error syncing resurrection status: {ex.Message}");
        }
    }

    #endregion

    #region Harmony 补丁点

    /// <summary>
    /// 玩家受到伤害后的补丁
    /// 在玩家生命值变为 0 时，如果处于联机模式且不允许真死，则恢复为 1
    /// 这是假死机制的核心：防止玩家真正死亡
    /// </summary>
    /// <param name="__instance">被补丁的 BattleController 实例</param>
    /// <remarks>
    /// 重要的补丁点：当伤害导致玩家 HP 变为 0 时，
    /// 在不允许真死的联机模式下，恢复 HP 到 1，触发假死处理
    /// </remarks>
    [HarmonyPatch(typeof(BattleController), "Damage")]
    [HarmonyPostfix]
    public static void Damage_Postfix(BattleController __instance)
    {
        try
        {
            if (__instance?.Player == null) return;

            PlayerUnit player = __instance.Player;

            // 只处理联机模式下的假死
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            // 如果玩家生命值为 0 且不允许真死，则进入假死状态
            if (player.Hp <= 0 && !AllowRealDeath)
            {
                // 使用反射恢复生命值到 1
                Traverse traverse = Traverse.Create(player);
                traverse.Method("Heal", 1).GetValue();
                HandleFakeDeath(player); // 处理假死逻辑
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in Damage_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 死亡屏幕构造函数补丁
    /// 在联机模式下，当玩家死亡时，如果不允许真死，则跳过死亡屏幕显示
    /// 转而进入假死状态，允许游戏继续进行
    /// </summary>
    /// <returns>SpireReturn 对象，如果是假死则返回 SpireReturn.Return() 跳过原方法</returns>
    /// <remarks>
    /// 这个补丁拦截死亡屏幕的显示，在假死时返回而不显示游戏结束界面
    /// </remarks>
    [HarmonyPatch(typeof(Unit), "Die")]
    [HarmonyPrefix]
    public static bool Die_Prefix(Unit __instance)
    {
        try
        {
            // 只处理玩家单位
            if (__instance is not PlayerUnit player) return true;

            // 只处理联机模式
            if (NetworkClient == null || !NetworkClient.IsConnected) return true;

            // 如果不允许真死，则进入假死状态而不真正死亡
            if (!AllowRealDeath && player.IsDead)
            {
                HandleFakeDeath(player);
                return false; // 阻止真正的死亡流程
            }

            return true; // 继续执行原有的死亡逻辑
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in Die_Prefix: {ex.Message}\n{ex.StackTrace}");
            return true; // 出错时继续执行
        }
    }

    /// <summary>
    /// BattleController 回合开始补丁
    /// 如果玩家处于假死状态，则跳过玩家回合逻辑
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "StartPlayerTurn")]
    [HarmonyPrefix]
    public static bool StartPlayerTurn_Prefix(BattleController __instance)
    {
        try
        {
            if (__instance?.Player == null) return true;

            // 假死状态下跳过回合开始处理
            if (IsPlayerInFakeDeath(__instance.Player))
            {
                Plugin.Logger?.LogDebug("[DeathPatch] Player turn start skipped - player in fake death");
                return false; // 阻止回合开始
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in StartPlayerTurn_Prefix: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// BattleController 卡牌使用请求补丁
    /// 如果玩家处于假死状态，则阻止出牌
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "RequestUseCard")]
    [HarmonyPrefix]
    public static bool RequestUseCard_Prefix(BattleController __instance, Card card)
    {
        try
        {
            if (__instance?.Player == null || card == null) return true;

            // 假死状态下不能出牌
            if (IsPlayerInFakeDeath(__instance.Player))
            {
                Plugin.Logger?.LogDebug("[DeathPatch] Card play blocked - player in fake death");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in RequestUseCard_Prefix: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// BattleController 结束回合请求补丁
    /// 如果玩家处于假死状态，则阻止结束回合（应当由系统自动跳过）
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "RequestEndPlayerTurn")]
    [HarmonyPrefix]
    public static bool RequestEndPlayerTurn_Prefix(BattleController __instance)
    {
        try
        {
            if (__instance?.Player == null) return true;

            // 假死状态下不能手动结束回合
            if (IsPlayerInFakeDeath(__instance.Player))
            {
                Plugin.Logger?.LogDebug("[DeathPatch] End turn request blocked - player in fake death");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in RequestEndPlayerTurn_Prefix: {ex.Message}");
            return true;
        }
    }

    /// <summary>
    /// 战斗结束条件检查补丁
    /// 检查游戏是否应该结束（所有玩家都死亡或战斗胜利）
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "BattleShouldEnd", MethodType.Getter)]
    [HarmonyPostfix]
    public static void BattleShouldEnd_Postfix(BattleController __instance, ref bool __result)
    {
        try
        {
            if (__instance?.Player == null) return;

            // 只处理联机模式
            if (NetworkClient == null || !NetworkClient.IsConnected)
            {
                return;
            }

            var player = __instance.Player;

            // 在假死状态下（玩家已死但不允许真死），游戏继续进行
            // 只有在允许真死或玩家活着时才检查真正的战斗结束条件
            if (player.IsDead && !AllowRealDeath)
            {
                __result = false; // 游戏不结束，等待复活或所有玩家死亡
                Plugin.Logger?.LogDebug("[DeathPatch] Battle continues - player in fake death");
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in BattleShouldEnd_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// 玩家输入等待状态检查补丁
    /// 如果玩家处于假死状态，则不等待玩家输入
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "IsWaitingPlayerInput", MethodType.Getter)]
    [HarmonyPrefix]
    public static bool IsWaitingPlayerInput_Prefix(BattleController __instance, ref bool __result)
    {
        try
        {
            if (__instance?.Player == null) return true;

            // 假死状态下不等待玩家输入
            if (IsPlayerInFakeDeath(__instance.Player))
            {
                __result = false;
                Plugin.Logger?.LogDebug("[DeathPatch] Player input wait cancelled - player in fake death");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[DeathPatch] Error in IsWaitingPlayerInput_Prefix: {ex.Message}");
            return true;
        }
    }

    #endregion

    #region 公共接口方法（供网络回调或其他模块调用）

    /// <summary>
    /// 公开方法：触发玩家复活事件
    /// 可由网络回调或 Gap 等特殊事件调用
    /// </summary>
    /// <param name="player">要复活的玩家</param>
    /// <param name="resurrectionHp">复活生命值，null 则为最大 HP 的 50%</param>
    public static void ResurrectPlayer(PlayerUnit player, int? resurrectionHp = null)
    {
        HandleResurrection(player, resurrectionHp);
    }

    /// <summary>
    /// 公开方法：设置是否允许真正死亡
    /// 当所有联机玩家都死亡时调用此方法
    /// </summary>
    /// <param name="allow">是否允许真正死亡</param>
    public static void SetAllowRealDeath(bool allow)
    {
        AllowRealDeath = allow;
        Plugin.Logger?.LogInfo($"[DeathPatch] AllowRealDeath set to: {allow}");
    }

    /// <summary>
    /// 公开方法：获取玩家死亡状态
    /// </summary>
    /// <param name="player">要检查的玩家</param>
    /// <returns>如果玩家处于假死状态则返回 true</returns>
    public static bool IsPlayerDead(PlayerUnit player)
    {
        return IsPlayerInFakeDeath(player);
    }

    #endregion
}
