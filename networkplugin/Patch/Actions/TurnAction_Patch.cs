using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Messages;

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
/// 重要说明：当前版本中的所有补丁方法都被注释，因为：
/// - LBoL游戏的Action类可能不包含Execute方法
/// - 或者方法名称、参数结构在不同版本中有所变化
/// - 需要进一步研究LBoL的API来确定正确的补丁目标
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

    /*
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
            var source = __instance.Source;
            var battle = source?.Battle;
            if (battle == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Battle is null for StartPlayerTurn");
                return;
            }

            // 只同步玩家回合 - 过滤敌人回合
            if (!(source is PlayerUnit))
            {
                return; // 敌人回合不进行同步
            }

            // 构建详细的回合开始同步数据
            var turnData = new
            {
                Timestamp = DateTime.Now.Ticks,                   // 事件时间戳
                PlayerId = source.Id,                             // 玩家唯一ID
                PlayerName = source.Name,                         // 玩家名称
                TurnNumber = battle.TurnCounter,                  // 当前回合数
                DrawCount = __instance.DrawCount,                 // 本回合抽牌数量
                ExtraTurn = __instance.ExtraTurn,                 // 是否为额外回合
                CardsInHand = battle.Player.HandZone.Count,      // 手牌数量
                CardsInDraw = battle.DrawZone.Count,              // 牌库数量
                CardsInDiscard = battle.DiscardZone.Count,        // 弃牌堆数量
                ManaReset = true,                                 // 回合开始时法力重置标志
                PlayerState = new                                 // 玩家当前状态
                {
                    Hp = source.Hp,                               // 当前HP
                    MaxHp = source.MaxHp,                         // 最大HP
                    Block = source.Block,                         // 当前格挡值
                    Shield = source.Shield,                       // 当前护盾值
                    Mana = battle.BattleMana != null ? GetManaGroup(battle.BattleMana) : [0, 0, 0, 0] // 法力值
                }
            };

            // 发送回合开始事件到网络服务器
            var json = JsonSerializer.Serialize(turnData);
            networkClient.SendRequest(NetworkMessageTypes.OnTurnStart, json);

            // 记录回合开始的详细日志
            Plugin.Logger?.LogInfo(
                $"[TurnSync] Player turn started. " +
                $"Player: {source.Name}, " +
                $"Turn #{turnData.TurnNumber}, " +
                $"Cards in hand: {turnData.CardsInHand}, " +
                $"Draw count: {turnData.DrawCount}");
        }
        catch (Exception ex)
        {
            // 捕获并记录异常，防止补丁错误影响游戏
            Plugin.Logger?.LogError($"[TurnSync] Error in StartPlayerTurn_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
    */

    /*
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
            // 验证服务提供者
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] ServiceProvider not initialized for EndPlayerTurn");
                return;
            }

            // 验证网络客户端
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Network client not available for EndPlayerTurn");
                return;
            }

            // 获取回合动作的来源单位和战斗信息
            var source = __instance.Source;
            var battle = source?.Battle;
            if (battle == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Battle is null for EndPlayerTurn");
                return;
            }

            // 只同步玩家回合
            if (!(source is PlayerUnit))
            {
                return; // 敌人回合不进行同步
            }

            // 构建回合结束的详细同步数据
            var turnData = new
            {
                Timestamp = DateTime.Now.Ticks,                   // 事件时间戳
                PlayerId = source.Id,                             // 玩家唯一ID
                PlayerName = source.Name,                         // 玩家名称
                TurnNumber = battle.TurnCounter,                  // 当前回合数
                CardsInHand = battle.Player.HandZone.Count,      // 手牌数量
                CardsInDiscard = battle.DiscardZone.Count,        // 弃牌堆数量
                CardsInExhaust = battle.ExhaustZone.Count,        // 放弃堆数量
                Block = source.Block,                             // 当前格挡值
                Shield = source.Shield,                           // 当前护盾值
                ExtraTurn = __instance.ExtraTurn,                 // 是否为额外回合
                TurnDuration = DateTime.Now.Ticks - turnStartTime, // 回合持续时间
                PlayerState = new                                 // 玩家最终状态
                {
                    Hp = source.Hp,                               // 当前HP
                    MaxHp = source.MaxHp,                         // 最大HP
                    IsAlive = source.IsAlive,                     // 存活状态
                    Mana = battle.BattleMana != null ? GetManaGroup(battle.BattleMana) : [0, 0, 0, 0] // 法力值
                }
            };

            // 发送回合结束事件到网络服务器
            var json = JsonSerializer.Serialize(turnData);
            networkClient.SendRequest(NetworkMessageTypes.OnTurnEnd, json);

            // 记录回合结束的详细日志
            Plugin.Logger?.LogInfo(
                $"[TurnSync] Player turn ended. " +
                $"Player: {source.Name}, " +
                $"Turn #{turnData.TurnNumber}, " +
                $"Cards in hand: {turnData.CardsInHand}, " +
                $"Block: {turnData.Block}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnSync] Error in EndPlayerTurn_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
    */

    /*
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
        try
        {
            // 验证服务提供者
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] ServiceProvider not initialized for StartBattle");
                return;
            }

            // 验证网络客户端
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Network client not available for StartBattle");
                return;
            }

            // 获取战斗实例
            var battle = __instance.Battle;
            if (battle == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Battle is null for StartBattle");
                return;
            }

            // 构建战斗开始的详细同步数据
            var battleData = new
            {
                Timestamp = DateTime.Now.Ticks,                   // 事件时间戳
                BattleId = battle.GetHashCode(),                  // 战斗唯一标识（临时实现）
                PlayerId = battle.Player.Id,                      // 玩家ID
                PlayerName = battle.Player.Name,                  // 玩家名称
                EnemyGroupId = battle.EnemyGroup.GetHashCode(),   // 敌人组标识
                EnemyCount = battle.EnemyGroup.Count,             // 敌人数量
                InitialPlayerHP = battle.Player.Hp,               // 玩家初始HP
                InitialPlayerMaxHp = battle.Player.MaxHp,         // 玩家初始最大HP
                InitialPlayerBlock = battle.Player.Block,         // 玩家初始格挡值
                InitialPlayerShield = battle.Player.Shield,       // 玩家初始护盾值
                BattleMapNode = battle.MapNode?.Id,               // 战斗所在地图节点
                EnemyTypes = GetEnemyTypes(battle.EnemyGroup),    // 敌人类型列表
                PlayerState = new                                 // 玩家完整状态
                {
                    Level = battle.Player.Level,                  // 玩家等级
                    Money = battle.Player.Money,                  // 玩家金钱
                    CardsInHand = battle.Player.HandZone.Count,   // 手牌数量
                    CardsInDraw = battle.DrawZone.Count,          // 牌库数量
                    Mana = battle.BattleMana != null ? GetManaGroup(battle.BattleMana) : [0, 0, 0, 0] // 法力值
                }
            };

            // 发送战斗开始事件到网络服务器
            var json = JsonSerializer.Serialize(battleData);
            networkClient.SendRequest(NetworkMessageTypes.OnBattleStart, json);

            // 记录战斗开始的详细日志
            Plugin.Logger?.LogInfo(
                $"[TurnSync] Battle started. " +
                $"Player: {battle.Player.Name}, " +
                $"Enemies: {battleData.EnemyCount}, " +
                $"Player HP: {battleData.InitialPlayerHP}/{battleData.InitialPlayerMaxHp}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnSync] Error in StartBattle_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
    */

    /*
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
        try
        {
            // 验证服务提供者
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] ServiceProvider not initialized for EndBattle");
                return;
            }

            // 验证网络客户端
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Network client not available for EndBattle");
                return;
            }

            // 获取战斗实例
            var battle = __instance.Battle;
            if (battle == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] Battle is null for EndBattle");
                return;
            }

            // 构建战斗结束的详细同步数据
            var battleData = new
            {
                Timestamp = DateTime.Now.Ticks,                   // 事件时间戳
                BattleId = battle.GetHashCode(),                  // 战斗唯一标识
                PlayerId = battle.Player.Id,                      // 玩家ID
                PlayerName = battle.Player.Name,                  // 玩家名称
                FinalPlayerHP = battle.Player.Hp,                 // 玩家最终HP
                FinalPlayerMaxHp = battle.Player.MaxHp,           // 玩家最终最大HP
                FinalPlayerBlock = battle.Player.Block,           // 玩家最终格挡值
                FinalPlayerShield = battle.Player.Shield,         // 玩家最终护盾值
                Victory = __instance.Victory,                     // 是否胜利
                Escape = __instance.Escape,                       // 是否逃跑
                BattleDuration = DateTime.Now.Ticks - battleStartTime, // 战斗持续时间
                TurnsPlayed = battle.TurnCounter,                 // 战斗回合数
                CardsPlayed = GetCardsPlayedCount(battle),        // 使用的卡牌数量
                PlayerState = new                                 // 玩家完整状态
                {
                    IsAlive = battle.Player.IsAlive,              // 存活状态
                    Level = battle.Player.Level,                  // 玩家等级
                    Money = battle.Player.Money,                  // 玩家金钱
                    CardsInHand = battle.Player.HandZone.Count,   // 手牌数量
                    CardsInDraw = battle.DrawZone.Count,          // 牌库数量
                    CardsInDiscard = battle.DiscardZone.Count     // 弃牌堆数量
                },
                Rewards = GetBattleRewards(__instance)           // 战斗奖励信息
            };

            // 发送战斗结束事件到网络服务器
            var json = JsonSerializer.Serialize(battleData);
            networkClient.SendRequest(NetworkMessageTypes.OnBattleEnd, json);

            // 记录战斗结束的详细日志
            Plugin.Logger?.LogInfo(
                $"[TurnSync] Battle ended. " +
                $"Player: {battle.Player.Name}, " +
                $"Victory: {battleData.Victory}, " +
                $"Final HP: {battleData.FinalPlayerHP}/{battleData.FinalPlayerMaxHp}, " +
                $"Turns: {battleData.TurnsPlayed}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnSync] Error in EndBattle_Postfix: {ex.Message}\n{ex.StackTrace}");
        }
    }
    */

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

    /// <summary>
    /// 获取战斗中使用的卡牌数量
    /// 用于战斗结束时的统计信息
    /// </summary>
    /// <param name="battle">战斗实例</param>
    /// <returns>使用的卡牌总数</returns>
    private static int GetCardsPlayedCount(Battle battle)
    {
        // TODO: 实现卡牌使用计数逻辑
        // 可能需要追踪战斗过程中的所有PlayCardAction
        return 0; // 临时返回值
    }

    /// <summary>
    /// 获取战斗奖励信息
    /// 用于战斗结束时的奖励同步
    /// </summary>
    /// <param name="endBattleAction">战斗结束动作实例</param>
    /// <returns>战斗奖励数据对象</returns>
    private static object GetBattleRewards(EndBattleAction endBattleAction)
    {
        // TODO: 实现战斗奖励提取逻辑
        // 包括金币、卡牌、宝物等奖励信息
        return new
        {
            MoneyReward = 0,      // TODO: 获取金钱奖励
            CardRewards = [],     // TODO: 获取卡牌奖励
            ExhibitRewards = []   // TODO: 获取宝物奖励
        };
    }
}