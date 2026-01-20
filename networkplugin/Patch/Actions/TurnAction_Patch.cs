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
using NetworkPlugin.Utils;

namespace NetworkPlugin.Patch.Actions;

/// <summary>
/// 回合/战斗关键动作同步补丁。
/// </summary>
/// <remarks>
/// 目标：在回合开始/结束、战斗开始/结束等关键节点，将本地状态打包并发送到服务器，
/// 以便远端玩家对齐时间线与状态。
/// </remarks>
public class TurnAction_Patch
{
    #region 依赖注入

    /// <summary>
    /// 依赖注入服务提供者（用于解析网络客户端服务）。
    /// </summary>
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    #endregion

    #region 玩家回合开始同步

    /// <summary>
    /// 玩家回合开始后置补丁：构建回合开始快照并发送。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="StartPlayerTurnAction"/> 实例（Harmony 注入）。</param>
    [HarmonyPatch(typeof(StartPlayerTurnAction), "Execute")]
    [HarmonyPostfix]
    public static void StartPlayerTurn_Postfix(StartPlayerTurnAction __instance)
    {
        try
        {
            // 依赖注入服务未就绪时跳过。
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] ServiceProvider 未初始化（StartPlayerTurn）");
                return;
            }

            // 获取网络客户端并确认连接。
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[TurnSync] 网络客户端不可用（StartPlayerTurn）");
                return;
            }

            // 取战斗上下文与本地玩家单位。
            BattleController battle = __instance.Unit?.Battle;
            if (battle == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] battle 为空（StartPlayerTurn）");
                return;
            }

            PlayerUnit source = battle.Player;
            if (source == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] battle.Player 为空（StartPlayerTurn）");
                return;
            }

            // 状态效果快照：此处保留空列表，后续可在合适的 API 点补充。
            var statusEffectsSnapshot = new List<StatusEffectStateSnapshot>();

            // 构建玩家状态快照。
            PlayerStateSnapshot playerStateSnapshot = new PlayerStateSnapshot
            {
                UserName = networkClient.GetSelf().userName,
                Health = source.Hp,
                MaxHealth = source.MaxHp,
                Block = source.Block,
                Shield = source.Shield,
                ManaGroup = ManaUtils.ManaGroupToArray(battle.BattleMana),
                MaxMana = 0, // TODO：后续补齐最大法力获取逻辑
                Gold = 0, // TODO：后续补齐金币获取逻辑
                TurnNumber = source.TurnCounter,
                IsInBattle = true,
                IsAlive = source.IsAlive,
                IsPlayersTurn = source is PlayerUnit,
                IsInTurn = source.IsInTurn,
                IsExtraTurn = source.IsExtraTurn,
                CharacterType = source is PlayerUnit ? "Player" : "Enemy",
                ReconnectToken = string.Empty,
                DisconnectTime = 0,
                LastUpdateTime = DateTime.Now.Ticks,
                IsAIControlled = false,
                TurnCounter = source.TurnCounter,
                Timestamp = DateTime.Now,
            };

            // 构建意图快照（由 IntentionSnapshot 内部根据战斗控制器提取）。
            IntentionSnapshot intentionSnapshot = new IntentionSnapshot(battleController: battle);

            // 构建回合开始同步数据。
            TurnStartStateSnapshot turnData = new TurnStartStateSnapshot(
                statusEffectStateSnapshot: statusEffectsSnapshot,
                playerStateSnapshot: playerStateSnapshot,
                intentionSnapshot: intentionSnapshot
            );

            // 序列化并发送回合开始事件。
            string json = JsonSerializer.Serialize(turnData);

            // TODO：服务器接收后需要将数据应用到对应的 INetworkPlayer。
            networkClient.SendRequest(NetworkMessageTypes.OnTurnStart, json);

            Plugin.Logger?.LogInfo("[TurnSync] 玩家回合开始已同步");
        }
        catch (Exception ex)
        {
            // 捕获异常，避免补丁异常影响回合流程。
            Plugin.Logger?.LogError($"[TurnSync] StartPlayerTurn_Postfix 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region 玩家回合结束同步

    /// <summary>
    /// 玩家回合结束后置补丁（预留）。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="EndPlayerTurnAction"/> 实例（Harmony 注入）。</param>
    [HarmonyPatch(typeof(EndPlayerTurnAction), "Execute")]
    [HarmonyPostfix]
    public static void EndPlayerTurn_Postfix(EndPlayerTurnAction __instance)
    {
        try
        {
            // 依赖注入服务未就绪时跳过。
            if (serviceProvider == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] ServiceProvider 未初始化（EndPlayerTurn）");
                return;
            }

            // 获取网络客户端并确认连接。
            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                Plugin.Logger?.LogDebug("[TurnSync] 网络客户端不可用（EndPlayerTurn）");
                return;
            }

            // 回合结束的“协商/锁定”逻辑由 EndTurnSyncPatch 处理（EndTurnRequest/Confirm）。
            // 当真正执行到 EndPlayerTurnAction 时，说明回合已实际结束；此处仅发送“回合边界快照”，不介入协商过程。

            // 取战斗上下文与本地玩家单位。
            BattleController battle = __instance.Unit?.Battle;
            if (battle == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] battle 为空（EndPlayerTurn）");
                return;
            }

            PlayerUnit source = battle.Player;
            if (source == null)
            {
                Plugin.Logger?.LogDebug("[TurnSync] battle.Player 为空（EndPlayerTurn）");
                return;
            }

            // 状态效果快照：此处保留空列表，后续可在合适的 API 点补充。
            var statusEffectsSnapshot = new List<StatusEffectStateSnapshot>();

            // 构建玩家状态快照（回合结束后的最终状态）。
            PlayerStateSnapshot playerStateSnapshot = new PlayerStateSnapshot
            {
                UserName = networkClient.GetSelf().userName,
                Health = source.Hp,
                MaxHealth = source.MaxHp,
                Block = source.Block,
                Shield = source.Shield,
                ManaGroup = ManaUtils.ManaGroupToArray(battle.BattleMana),
                MaxMana = 0, // TODO：后续补齐最大法力获取逻辑
                Gold = 0, // TODO：后续补齐金币获取逻辑
                TurnNumber = source.TurnCounter,
                IsInBattle = true,
                IsAlive = source.IsAlive,
                IsPlayersTurn = false,
                IsInTurn = source.IsInTurn,
                IsExtraTurn = source.IsExtraTurn,
                CharacterType = source is PlayerUnit ? "Player" : "Enemy",
                ReconnectToken = string.Empty,
                DisconnectTime = 0,
                LastUpdateTime = DateTime.Now.Ticks,
                IsAIControlled = false,
                TurnCounter = source.TurnCounter,
                Timestamp = DateTime.Now,
            };

            // 构建意图快照（回合结束后通常进入敌方回合；意图可能已变化，仍以当前战斗控制器提取为准）。
            IntentionSnapshot intentionSnapshot = new IntentionSnapshot(battleController: battle);

            // 复用 EndTurnSyncPatch 的 battleId 生成策略，保证跨客户端一致。
            string battleId = "battle";
            try
            {
                var run = GameStateUtils.GetCurrentGameRun();
                var node = run?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    battleId = $"Act{node.Act}:{node.X}:{node.Y}:{node.StationType}";
                }
            }
            catch
            {
                // ignored
            }

            int round = battle.RoundCounter;

            // 构建回合结束同步数据。
            TurnEndStateSnapshot turnEndData = new TurnEndStateSnapshot(
                statusEffectStateSnapshot: statusEffectsSnapshot,
                playerStateSnapshot: playerStateSnapshot,
                intentionSnapshot: intentionSnapshot,
                battleId: battleId,
                round: round
            );

            // 序列化并发送回合结束事件。
            string json = JsonSerializer.Serialize(turnEndData);
            networkClient.SendRequest(NetworkMessageTypes.OnTurnEnd, json);

            Plugin.Logger?.LogInfo("[TurnSync] 玩家回合结束已同步");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnSync] EndPlayerTurn_Postfix 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    #endregion

    #region 战斗开始/结束同步（预留）

    /// <summary>
    /// 战斗开始后置补丁（预留）。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="StartBattleAction"/> 实例（Harmony 注入）。</param>
    [HarmonyPatch(typeof(StartBattleAction), "Execute")]
    [HarmonyPostfix]
    public static void StartBattle_Postfix(StartBattleAction __instance)
    {
        // TODO：待确认 StartBattle 的可用字段与同步内容。
    }

    /// <summary>
    /// 战斗结束后置补丁（预留）。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="EndBattleAction"/> 实例（Harmony 注入）。</param>
    [HarmonyPatch(typeof(EndBattleAction), "Execute")]
    [HarmonyPostfix]
    public static void EndBattle_Postfix(EndBattleAction __instance)
    {
        // TODO：待确认 EndBattle 的可用字段与同步内容。
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 将 <see cref="ManaGroup"/> 转换为整数数组，用于网络序列化。
    /// </summary>
    /// <param name="manaGroup">法力组对象。</param>
    /// <returns>数组格式：<c>[红, 蓝, 绿, 白]</c>。</returns>
    private static int[] GetManaGroup(ManaGroup manaGroup)
    {
        if (manaGroup == null)
        {
            // 默认值：所有颜色法力为 0。
            return [0, 0, 0, 0];
        }

        // 按颜色顺序输出。
        return
        [
            manaGroup.Red,
            manaGroup.Blue,
            manaGroup.Green,
            manaGroup.White,
        ];
    }

    /// <summary>
    /// 将 <see cref="ManaGroup"/> 转换为字典形式（颜色 -> 数量），用于网络序列化。
    /// </summary>
    /// <param name="manaGroup">法力组对象。</param>
    /// <returns>颜色到数量的映射字典。</returns>
    private static Dictionary<ManaColor, int> ConvertManaGroupToDictionary(ManaGroup manaGroup)
    {
        // 空法力组直接返回空字典。
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
            [ManaColor.Hybrid] = manaGroup.Hybrid,
        };
    }

    /// <summary>
    /// 从敌人组中提取敌人类型名称列表，用于战斗开始同步。
    /// </summary>
    /// <param name="enemyGroup">敌人组。</param>
    /// <returns>敌人名称数组。</returns>
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

    #endregion
}
