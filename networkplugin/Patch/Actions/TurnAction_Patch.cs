using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;
using NetworkPlugin.Network.Snapshot;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// 回合管理同步补丁类
/// 使用Harmony框架拦截LBoL游戏的回合和战斗管理逻辑
/// 实现回合开始、结束以及战斗状态的网络同步功能
/// </summary>
/// <remarks>
/// 这个类负责同步游戏中的回合制关键事件，包括：
/// 1. 玩家回合的开始和结束
/// 2. 战斗的启动和结束
/// 3. 回合间的状态转换
///
///
/// 这些注释的代码展示了回合同步的实现思路，可在找到正确的API后重新启用
/// </remarks>
public class TurnAction_Patch
{
    /// <summary>
    /// 服务提供者实例，用于获取依赖注入的网络客户端服务
    /// </summary>
    private static IServiceProvider serviceProvider =>
        ModService.ServiceProvider;


    /// <summary>
    /// 玩家回合开始事件同步补丁
    /// 在StartPlayerTurnAction方法执行完成后被调用
    /// 同步玩家回合开始时的游戏状态，包括回合数、抽牌数量、法力重置等信息
    /// </summary>
    /// <param name="__instance">被补丁的StartPlayerTurnAction实例（Harmony自动注入）</param>
    /// <remarks>
    /// 当前被注释的原因：
    /// - StartPlayerTurnAction类可能不存在Execute方法
    /// - 或者方法签名与预期不符
    /// - 需要查找LBoL源码确定正确的拦截点
    ///
    /// 同步的信息包括：
    /// - 玩家ID和回合数
    /// - 抽牌数量和额外回合状态
    /// - 牌库状态（手牌、牌库、弃牌堆）
    /// - 法力重置标志
    ///
    /// 发送的事件类型：OnTurnStart
    /// </remarks>
    [HarmonyPatch(typeof(StartPlayerTurnAction), "Execute")]
    [HarmonyPostfix]
    public static void StartPlayerTurn_Postfix(StartPlayerTurnAction __instance)
    {
        try
        {
            // 验证服务提供者是否已初始化
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] ServiceProvider not initialized for StartPlayerTurn");
                return;
            }

            // 获取并验证网络客户端
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Network client not available for StartPlayerTurn");
                return;
            }

            // 获取回合动作的来源单位和战斗信息

            BattleController battle = __instance.Unit?.Battle;
            PlayerUnit source = battle.Player;
            if (battle == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Battle is null for StartPlayerTurn");
                return;
            }

            // 构建状态效果快照列表
            var statusEffectsSnapshot = new List<StatusEffectStateSnapshot>();

            // 构建玩家状态快照
            PlayerStateSnapshot playerStateSnapshot = new PlayerStateSnapshot(
                userName: networkClient.GetSelf().userName,
                health: source.Hp,
                maxHealth: source.MaxHp,
                block: source.Block,
                shield: source.Shield,
                mana: ConvertManaGroupToDictionary(battle.BattleMana),
                maxMana: 0, // TODO: 获取最大法力值
                gold: 0,
                turnNumber: source.TurnCounter,
                isInBattle: battle != null,
                isAlive: source.IsAlive,
                isPlayersTurn: source is PlayerUnit,
                isInTurn: source.IsInTurn,
                isExtraTurn: source.IsExtraTurn,
                characterType: source is PlayerUnit ? "Player" : "Enemy",
                reconnectToken: "",
                disconnectTime: 0,
                lastUpdateTime: DateTime.Now.Ticks,
                isAIControlled: false,
                turnCounter: source.TurnCounter,
                timestamp: DateTime.Now
            );

            // 构建意图快照 - 传递战斗控制器，让IntentionSnapshot类内部处理意图提取
            IntentionSnapshot intentionSnapshot = new IntentionSnapshot(battleController: battle);

            // 构建详细的回合开始同步数据
            TurnStartStateSnapshot turnData = new TurnStartStateSnapshot(
                statusEffectStateSnapshot: statusEffectsSnapshot,
                playerStateSnapshot: playerStateSnapshot,
                intentionSnapshot: intentionSnapshot
            );


            // 发送回合开始事件到网络服务器
            var json = JsonSerializer.Serialize(turnData);
            //TODO:服务器接收需要将数据应用变更至INetworkPlayer
            networkClient.SendRequest(NetworkMessageTypes.OnTurnStart, json);


            // 记录回合开始的详细日志
            Plugin.Logger?.LogInfo(
                $"[TurnSync] Player turn started. "

            );
        }
        catch (Exception ex)
        {
            // 捕获并记录异常，防止补丁错误影响游戏
            Plugin.Logger?.LogError($"[TurnSync] Error in StartPlayerTurn_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }



    /// <summary>
    /// 玩家回合结束事件同步补丁
    /// 在EndPlayerTurnAction方法执行完成后被调用
    /// 同步玩家回合结束时的最终状态，包括手牌数量、格挡值等状态信息
    /// </summary>
    /// <param name="__instance">被补丁的EndPlayerTurnAction实例（Harmony自动注入）</param>
    /// <remarks>
    /// 当前被注释的原因：
    /// - EndPlayerTurnAction类可能不存在Execute方法
    /// - 或者方法签名与预期不符
    /// - 需要查找LBoL源码确定正确的拦截点
    ///
    /// 同步的信息包括：
    /// - 玩家ID和回合数
    /// - 牌库状态（手牌、弃牌堆）
    /// - 玩家防御状态（格挡值、护盾值）
    /// - 额外回合标志
    ///
    /// 发送的事件类型：OnTurnEnd
    /// </remarks>
    [HarmonyPatch(typeof(EndPlayerTurnAction), "Execute")]
    [HarmonyPostfix]
    public static void EndPlayerTurn_Postfix(EndPlayerTurnAction __instance)
    {
        try
        {
            //TODO:待定
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnSync] Error in EndPlayerTurn_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }



    /// <summary>
    /// 战斗开始事件同步补丁
    /// 在StartBattleAction方法执行完成后被调用
    /// 同步战斗开始时的初始状态，包括玩家状态和敌人信息
    /// </summary>
    /// <param name="__instance">被补丁的StartBattleAction实例（Harmony自动注入）</param>
    /// <remarks>
    /// 当前被注释的原因：
    /// - StartBattleAction类可能不存在Execute方法
    /// - 或者方法签名与预期不符
    /// - 需要查找LBoL源码确定正确的拦截点
    ///
    /// 同步的信息包括：
    /// - 战斗唯一标识
    /// - 玩家信息和初始状态
    /// - 敌人组信息和数量
    /// - 初始HP、格挡、护盾值
    ///
    /// 发送的事件类型：OnBattleStart
    /// </remarks>
    [HarmonyPatch(typeof(StartBattleAction), "Execute")]
    [HarmonyPostfix]
    public static void StartBattle_Postfix(StartBattleAction __instance)
    {

    }



    /// <summary>
    /// 战斗结束事件同步补丁
    /// 在EndBattleAction方法执行完成后被调用
    /// 同步战斗结束时的最终结果，包括胜利状态、玩家最终状态等信息
    /// </summary>
    /// <param name="__instance">被补丁的EndBattleAction实例（Harmony自动注入）</param>
    /// <remarks>
    /// 当前被注释的原因：
    /// - EndBattleAction类可能不存在Execute方法
    /// - 或者方法签名与预期不符
    /// - 需要查找LBoL源码确定正确的拦截点
    ///
    /// 同步的信息包括：
    /// - 战斗标识和结果状态
    /// - 玩家最终HP和状态
    /// - 胜利/逃跑标志
    /// - 战斗奖励信息
    ///
    /// 发送的事件类型：OnBattleEnd
    /// </remarks>
    [HarmonyPatch(typeof(EndBattleAction), "Execute")]
    [HarmonyPostfix]
    public static void EndBattle_Postfix(EndBattleAction __instance)
    {

    }


    // ========================================
    // 辅助方法 - 用于数据转换（注释掉的补丁使用的工具方法）
    // ========================================

    /// <summary>
    /// 将ManaGroup对象转换为整数数组
    /// 用于网络传输中的法力值序列化
    /// </summary>
    /// <param name="manaGroup">法力组对象</param>
    /// <returns>包含四种颜色法力值的数组 [红, 蓝, 绿, 白]</returns>
    private static int[] GetManaGroup(ManaGroup manaGroup)
    {
        if (manaGroup == null)
        {
            return [0, 0, 0, 0]; // 默认值：所有颜色法力为0
        }

        return
        [
        manaGroup.Red,    // 红色法力（火）
        manaGroup.Blue,   // 蓝色法力（水）
        manaGroup.Green,  // 绿色法力（木）
        manaGroup.White   // 白色法力（光）
        ];
    }

    /// <summary>
    /// 将ManaGroup对象转换为Dictionary&lt;ManaColor, int&gt;
    /// 用于网络传输中的法力值序列化
    /// </summary>
    /// <param name="manaGroup">法力组对象</param>
    /// <returns>包含所有颜色法力值的字典</returns>
    private static Dictionary<ManaColor, int> ConvertManaGroupToDictionary(ManaGroup manaGroup)
    {
        if (manaGroup.IsEmpty)
        {
            return [];
        }

        return new Dictionary<ManaColor, int>
        {
            [ManaColor.Any] = manaGroup.Any,
            [ManaColor.White] = manaGroup.White,
            [ManaColor.Blue] = manaGroup.Blue,
            [ManaColor.Black] = manaGroup.Black,
            [ManaColor.Red] = manaGroup.Red,
            [ManaColor.Green] = manaGroup.Green,
            [ManaColor.Colorless] = manaGroup.Colorless,
            [ManaColor.Philosophy] = manaGroup.Philosophy,
            [ManaColor.Hybrid] = manaGroup.Hybrid
        };
    }

    /// <summary>
    /// 获取敌人组中的敌人类型列表
    /// 用于战斗开始时的敌人信息同步
    /// </summary>
    /// <param name="enemyGroup">敌人组</param>
    /// <returns>敌人类型名称列表</returns>
    private static string[] GetEnemyTypes(IEnumerable<EnemyUnit> enemyGroup)
    {
        if (enemyGroup == null)
        {
            return [];
        }

        var enemyTypes = new List<string>();
        foreach (var enemy in enemyGroup)
        {
            enemyTypes.Add(enemy?.Name ?? "Unknown");
        }
        return enemyTypes.ToArray();
    }


}