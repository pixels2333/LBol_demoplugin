using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network;
using NetworkPlugin.Network.Client;
using NetworkPlugin.Network.Reconnection;
using NetworkPlugin.Utils;
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

            // 确保已追踪到服务器分配的 PlayerId（用于跨客户端定位 INetworkPlayer）。
            NetworkIdentityTracker.EnsureSubscribed(networkClient);

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

            GameRunController run = battle.GameRun ?? GameStateUtils.GetCurrentGameRun();
            string selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfPlayerId))
            {
                // 兼容：还未拿到 Welcome 时，先回退到角色 Id（不保证跨客户端唯一）。
                selfPlayerId = GameStateUtils.GetCurrentPlayerId();
            }

            int gold = 0;
            int maxMana = 0;
            try
            {
                gold = run?.Money ?? 0;
                maxMana = run != null ? ManaUtils.GetTotalMana(run.BaseMana) : 0;
            }
            catch
            {
                gold = 0;
                maxMana = 0;
            }

            // 状态效果快照：同步本回合开始时玩家身上的状态效果（轻量；不直接回放，只用于展示/诊断/对齐）。
            List<StatusEffectStateSnapshot> statusEffectsSnapshot = CaptureStatusEffects(source);

            // 构建玩家状态快照。
            PlayerStateSnapshot playerStateSnapshot = new PlayerStateSnapshot
            {
                PlayerId = selfPlayerId,
                UserName = networkClient.GetSelf().userName,
                Health = source.Hp,
                MaxHealth = source.MaxHp,
                Block = source.Block,
                Shield = source.Shield,
                ManaGroup = ManaUtils.ManaGroupToArray(battle.BattleMana),
                MaxMana = maxMana,
                Gold = gold,
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

            // 位置快照：与 ReconnectionManager 的恢复口径一致。
            try
            {
                var node = run?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    playerStateSnapshot.GameLocation = new LocationSnapshot
                    {
                        X = node.X,
                        Y = node.Y,
                        NodeId = $"Act{node.Act}:{node.X}:{node.Y}:{node.StationType}",
                        NodeType = node.StationType.ToString(),
                        VisitTime = DateTime.UtcNow.Ticks,
                    };
                }
            }
            catch
            {
                // ignored
            }

            // 构建意图快照（由 IntentionSnapshot 内部根据战斗控制器提取）。
            IntentionSnapshot intentionSnapshot = new IntentionSnapshot(battleController: battle);

            // 构建回合开始同步数据。
            TurnStartStateSnapshot turnData = new TurnStartStateSnapshot(
                statusEffectStateSnapshot: statusEffectsSnapshot,
                playerStateSnapshot: playerStateSnapshot,
                intentionSnapshot: intentionSnapshot
            );

            // 序列化并发送回合开始事件。
            string json = JsonCompat.Serialize(turnData);

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

            NetworkIdentityTracker.EnsureSubscribed(networkClient);

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

            GameRunController run = battle.GameRun ?? GameStateUtils.GetCurrentGameRun();
            string selfPlayerId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfPlayerId))
            {
                selfPlayerId = GameStateUtils.GetCurrentPlayerId();
            }

            int gold = 0;
            int maxMana = 0;
            try
            {
                gold = run?.Money ?? 0;
                maxMana = run != null ? ManaUtils.GetTotalMana(run.BaseMana) : 0;
            }
            catch
            {
                gold = 0;
                maxMana = 0;
            }

            // 状态效果快照：回合结束时玩家身上的状态效果。
            List<StatusEffectStateSnapshot> statusEffectsSnapshot = CaptureStatusEffects(source);

            // 构建玩家状态快照（回合结束后的最终状态）。
            PlayerStateSnapshot playerStateSnapshot = new PlayerStateSnapshot
            {
                PlayerId = selfPlayerId,
                UserName = networkClient.GetSelf().userName,
                Health = source.Hp,
                MaxHealth = source.MaxHp,
                Block = source.Block,
                Shield = source.Shield,
                ManaGroup = ManaUtils.ManaGroupToArray(battle.BattleMana),
                MaxMana = maxMana,
                Gold = gold,
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

            try
            {
                var node = run?.CurrentMap?.VisitingNode;
                if (node != null)
                {
                    playerStateSnapshot.GameLocation = new LocationSnapshot
                    {
                        X = node.X,
                        Y = node.Y,
                        NodeId = $"Act{node.Act}:{node.X}:{node.Y}:{node.StationType}",
                        NodeType = node.StationType.ToString(),
                        VisitTime = DateTime.UtcNow.Ticks,
                    };
                }
            }
            catch
            {
                // ignored
            }

            // 构建意图快照（回合结束后通常进入敌方回合；意图可能已变化，仍以当前战斗控制器提取为准）。
            IntentionSnapshot intentionSnapshot = new IntentionSnapshot(battleController: battle);

            // 复用 EndTurnSyncPatch 的 battleId 生成策略，保证跨客户端一致。
            string battleId = GetBattleId(run);

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
            string json = JsonCompat.Serialize(turnEndData);
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
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(networkClient);
            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                // 房主权威：仅房主广播战斗边界事件。
                return;
            }

            BattleController battle = __instance?.Battle;
            if (battle == null || battle.Player == null)
            {
                return;
            }

            // 只同步本地玩家触发的战斗。
            if (battle.Player != GameStateUtils.GetCurrentPlayer())
            {
                return;
            }

            GameRunController run = battle.GameRun ?? GameStateUtils.GetCurrentGameRun();
            string battleId = GetBattleId(run);

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfId))
            {
                selfId = GameStateUtils.GetCurrentPlayerId();
            }

            BattleStateSnapshot battleState = new BattleStateSnapshot
            {
                IsInBattle = true,
                BattleId = battleId,
                CurrentTurn = Math.Max(1, battle.RoundCounter),
                CurrentTurnPlayerId = selfId,
                TurnPhase = "Player",
                Enemies = CaptureEnemies(battle),
                BattleStartTime = DateTime.UtcNow.Ticks,
                BattleType = run?.CurrentMap?.VisitingNode?.StationType.ToString() ?? "Unknown",
            };

            try
            {
                string t = battleState.BattleType;
                battleState.IsBossBattle = !string.IsNullOrWhiteSpace(t) && t.IndexOf("Boss", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                battleState.IsBossBattle = false;
            }

            var payload = new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                SenderId = selfId,
                BattleId = battleId,
                BattleState = battleState,
            };

            networkClient.SendGameEventData(NetworkMessageTypes.OnBattleStart, payload);
            Plugin.Logger?.LogInfo($"[TurnSync] 战斗开始已同步: {battleId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnSync] StartBattle_Postfix 异常: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// 战斗结束后置补丁（预留）。
    /// </summary>
    /// <param name="__instance">被补丁的 <see cref="EndBattleAction"/> 实例（Harmony 注入）。</param>
    [HarmonyPatch(typeof(EndBattleAction), "Execute")]
    [HarmonyPostfix]
    public static void EndBattle_Postfix(EndBattleAction __instance)
    {
        try
        {
            if (serviceProvider == null)
            {
                return;
            }

            var networkClient = serviceProvider.GetService<INetworkClient>();
            if (networkClient == null || !networkClient.IsConnected)
            {
                return;
            }

            NetworkIdentityTracker.EnsureSubscribed(networkClient);
            if (!NetworkIdentityTracker.GetSelfIsHost())
            {
                return;
            }

            BattleController battle = __instance?.Battle;
            if (battle == null || battle.Player == null)
            {
                return;
            }

            if (battle.Player != GameStateUtils.GetCurrentPlayer())
            {
                return;
            }

            GameRunController run = battle.GameRun ?? GameStateUtils.GetCurrentGameRun();
            string battleId = GetBattleId(run);

            string selfId = NetworkIdentityTracker.GetSelfPlayerId();
            if (string.IsNullOrWhiteSpace(selfId))
            {
                selfId = GameStateUtils.GetCurrentPlayerId();
            }

            BattleStateSnapshot battleState = new BattleStateSnapshot
            {
                IsInBattle = false,
                BattleId = battleId,
                CurrentTurn = Math.Max(1, battle.RoundCounter),
                CurrentTurnPlayerId = selfId,
                TurnPhase = "Finished",
                Enemies = CaptureEnemies(battle),
                BattleStartTime = 0,
                BattleType = run?.CurrentMap?.VisitingNode?.StationType.ToString() ?? "Unknown",
            };

            var payload = new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                SenderId = selfId,
                BattleId = battleId,
                BattleState = battleState,
            };

            try
            {
                // Host-only: record a key checkpoint for mid-game join / reconnection.
                var reconnection = serviceProvider.GetService<ReconnectionManager>();
                string nodeKey = run?.CurrentMap?.VisitingNode != null
                    ? $"{run.CurrentMap.VisitingNode.Act}:{run.CurrentMap.VisitingNode.X}:{run.CurrentMap.VisitingNode.Y}:{run.CurrentMap.VisitingNode.StationType}"
                    : null;
                reconnection?.MarkMapCheckpoint("battle_end", nodeKey);
            }
            catch
            {
                // ignored
            }

            networkClient.SendGameEventData(NetworkMessageTypes.OnBattleEnd, payload);
            Plugin.Logger?.LogInfo($"[TurnSync] 战斗结束已同步: {battleId}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[TurnSync] EndBattle_Postfix 异常: {ex.Message}\n{ex.StackTrace}");
        }
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

    private static string GetBattleId(GameRunController run)
    {
        string battleId = "battle";
        try
        {
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

        return battleId;
    }

    private static List<StatusEffectStateSnapshot> CaptureStatusEffects(Unit unit)
    {
        var result = new List<StatusEffectStateSnapshot>();
        if (unit == null)
        {
            return result;
        }

        try
        {
            foreach (StatusEffect se in unit.StatusEffects)
            {
                if (se == null)
                {
                    continue;
                }

                int level = 0;
                int duration = 0;
                int value = 0;
                bool isPermanent = false;

                try
                {
                    if (se.HasLevel)
                    {
                        level = se.Level;
                    }
                }
                catch
                {
                    level = 0;
                }

                try
                {
                    if (se.HasDuration)
                    {
                        duration = se.Duration;
                        isPermanent = false;
                    }
                    else
                    {
                        duration = 0;
                        isPermanent = true;
                    }
                }
                catch
                {
                    duration = 0;
                }

                try
                {
                    if (se.HasCount)
                    {
                        value = se.Count;
                    }
                }
                catch
                {
                    value = 0;
                }

                string type = "Unknown";
                bool isDebuff = false;
                try
                {
                    type = se.Type.ToString();
                    isDebuff = se.Type == StatusEffectType.Negative;
                }
                catch
                {
                    type = "Unknown";
                    isDebuff = false;
                }

                result.Add(new StatusEffectStateSnapshot
                {
                    EffectId = se.Id ?? string.Empty,
                    EffectName = se.Name ?? string.Empty,
                    EffectType = type,
                    Level = level,
                    Duration = duration,
                    IsDebuff = isDebuff,
                    IsPermanent = isPermanent,
                    EffectValue = value,
                    Description = se.Description ?? string.Empty,
                    SourceId = string.Empty,
                });
            }
        }
        catch
        {
            // ignored
        }

        return result;
    }

    private static List<EnemyStateSnapshot> CaptureEnemies(BattleController battle)
    {
        var enemies = new List<EnemyStateSnapshot>();
        if (battle?.EnemyGroup == null)
        {
            return enemies;
        }

        try
        {
            int idx = 0;
            foreach (EnemyUnit e in battle.EnemyGroup)
            {
                if (e == null)
                {
                    idx++;
                    continue;
                }

                enemies.Add(new EnemyStateSnapshot
                {
                    EnemyId = e.Id,
                    EnemyName = e.Name,
                    EnemyType = e.GetType().Name,
                    Health = e.Hp,
                    MaxHealth = e.MaxHp,
                    Block = e.Block,
                    Shield = e.Shield,
                    StatusEffects = CaptureStatusEffects(e),
                    Intention = CaptureEnemyIntention(e),
                    Index = idx,
                    IsAlive = e.IsAlive,
                });

                idx++;
            }
        }
        catch
        {
            // ignored
        }

        return enemies;
    }

    private static IntentionSnapshot CaptureEnemyIntention(EnemyUnit enemy)
    {
        try
        {
            if (enemy?.Intentions == null)
            {
                return new IntentionSnapshot();
            }

            var i = enemy.Intentions.FirstOrDefault(x => x != null);
            if (i == null)
            {
                return new IntentionSnapshot();
            }

            return new IntentionSnapshot
            {
                IntentionType = i.Type.ToString(),
                IntentionName = i.Name ?? string.Empty,
                Description = i.Description ?? string.Empty,
                Value = 0,
            };
        }
        catch
        {
            return new IntentionSnapshot();
        }
    }

    #endregion
}
