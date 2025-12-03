using System;
using System.Text.Json;
using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using Microsoft.Extensions.DependencyInjection;
using NetworkPlugin.Network.Client;

namespace NetworkPlugin.Patch.Network;

/// <summary>
/// 法力管理同步补丁 - 同步法力增减、上限变更、法力获取
/// 参考杀戮尖塔EnergyManagerPatch，但适配LBoL的多色法力系统
/// 重要性: ⭐⭐⭐⭐⭐ (战斗核心机制)
/// </summary>
public class EnergySyncPatch
{
    private static IServiceProvider serviceProvider => ModService.ServiceProvider;

    /// <summary>
    /// ConsumeManaAction同步补丁 - 法力消耗同步
    /// </summary>
    [HarmonyPatch(typeof(ConsumeManaAction), "MainPhase")]
    [HarmonyPrefix]
    public static void ConsumeManaAction_MainPhase_Prefix(ConsumeManaAction __instance)
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

            var battle = __instance.Battle;
            if (battle?.BattleMana == null)
            {
                return;
            }

            var manaBefore = new
            {
                Red = battle.BattleMana.Red,
                Blue = battle.BattleMana.Blue,
                Green = battle.BattleMana.Green,
                White = battle.BattleMana.White,
                Total = battle.BattleMana.Total
            };

            var consumeManaData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ManaConsumeStarted",
                ManaBefore = manaBefore,
                ManaToConsume = __instance.Args.Value != null ? new
                {
                    Red = __instance.Args.Value.Red,
                    Blue = __instance.Args.Value.Blue,
                    Green = __instance.Args.Value.Green,
                    White = __instance.Args.Value.White,
                    Total = __instance.Args.Value.Total
                } : new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 },
                Source = "ConsumeManaAction"
            };

            var json = JsonSerializer.Serialize(consumeManaData);
            networkClient.SendRequest("ManaConsumeStarted", json);

            Plugin.Logger?.LogInfo($"[EnergySync] Mana consume started: R{__instance.Args.Value?.Red}B{__instance.Args.Value?.Blue}G{__instance.Args.Value?.Green}W{__instance.Args.Value?.White}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnergySync] Error in ConsumeManaAction_MainPhase_Prefix: {ex.Message}");
        }
    }

    [HarmonyPatch(typeof(ConsumeManaAction), "MainPhase")]
    [HarmonyPostfix]
    public static void ConsumeManaAction_MainPhase_Postfix(ConsumeManaAction __instance)
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

            var battle = __instance.Battle;
            if (battle?.BattleMana == null)
            {
                return;
            }

            var manaAfter = new
            {
                Red = battle.BattleMana.Red,
                Blue = battle.BattleMana.Blue,
                Green = battle.BattleMana.Green,
                White = battle.BattleMana.White,
                Total = battle.BattleMana.Total
            };

            var consumeManaData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "ManaConsumeCompleted",
                ManaAfter = manaAfter,
                ManaConsumed = __instance.Args.Value != null ? new
                {
                    Red = __instance.Args.Value.Red,
                    Blue = __instance.Args.Value.Blue,
                    Green = __instance.Args.Value.Green,
                    White = __instance.Args.Value.White,
                    Total = __instance.Args.Value.Total
                } : new { Red = 0, Blue = 0, Green = 0, White = 0, Total = 0 },
                Source = "ConsumeManaAction"
            };

            var json = JsonSerializer.Serialize(consumeManaData);
            networkClient.SendRequest("ManaConsumeCompleted", json);

            Plugin.Logger?.LogInfo($"[EnergySync] Mana consume completed. Current: R{manaAfter.Red}B{manaAfter.Blue}G{manaAfter.Green}W{manaAfter.White}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnergySync] Error in ConsumeManaAction_MainPhase_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// BattleController回合开始法力重置同步补丁
    /// </summary>
    [HarmonyPatch(typeof(BattleController), "TurnMana", MethodType.Getter)]
    [HarmonyPostfix]
    public static void BattleController_TurnMana_Postfix(BattleController __instance, ref ManaGroup __result)
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

            // 只在回合开始时同步一次（检查法力状态变化）
            // 这里可能需要添加额外的逻辑来避免重复发送

            var turnManaData = new
            {
                Timestamp = DateTime.Now.Ticks,
                EventType = "TurnManaCalculated",
                TurnMana = new
                {
                    Red = __result.Red,
                    Blue = __result.Blue,
                    Green = __result.Green,
                    White = __result.White,
                    Total = __result.Total
                },
                BaseTurnMana = new
                {
                    Red = __instance.BaseTurnMana.Red,
                    Blue = __instance.BaseTurnMana.Blue,
                    Green = __instance.BaseTurnMana.Green,
                    White = __instance.BaseTurnMana.White,
                    Total = __instance.BaseTurnMana.Total
                },
                ExtraTurnMana = new
                {
                    Red = __instance.ExtraTurnMana.Red,
                    Blue = __instance.ExtraTurnMana.Blue,
                    Green = __instance.ExtraTurnMana.Green,
                    White = __instance.ExtraTurnMana.White,
                    Total = __instance.ExtraTurnMana.Total
                },
                LockedTurnMana = new
                {
                    Red = __instance.LockedTurnMana.Red,
                    Blue = __instance.LockedTurnMana.Blue,
                    Green = __instance.LockedTurnMana.Green,
                    White = __instance.LockedTurnMana.White,
                    Total = __instance.LockedTurnMana.Total
                }
            };

            var json = JsonSerializer.Serialize(turnManaData);
            networkClient.SendRequest("TurnManaCalculated", json);

            Plugin.Logger?.LogDebug($"[EnergySync] Turn mana calculated: R{__result.Red}B{__result.Blue}G{__result.Green}W{__result.White}");
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[EnergySync] Error in BattleController_TurnMana_Postfix: {ex.Message}");
        }
    }

    /// <summary>
    /// TODO: 找到能量变更的Patch点
    /// 需要Patch游戏中所有改变能量的地方
    /// </summary>
    public class ManaGroupPatch
    {
        // LBoL使用ManaGroup管理法力颜色（红蓝绿白）
        // 需要找到ManaGroup值变更的位置

        // TODO: Patch BattleMana的ManaGroup setter
        // TODO: Patch PlayerUnit的回复法力方法
        // TODO: Patch 卡牌使用时的法力消耗

        public static void SyncManaGroupChange(ManaGroup oldGroup, ManaGroup newGroup, string changeReason)
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

                // TODO: 检查是否是当前玩家回合的操作
                // 在LBoL中，不同角色可能有不同的法力管理机制

                var manaData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    ChangeReason = changeReason,
                    OldGroup = new
                    {
                        Red = oldGroup?.Red ?? 0,
                        Blue = oldGroup?.Blue ?? 0,
                        Green = oldGroup?.Green ?? 0,
                        White = oldGroup?.White ?? 0,
                        Total = oldGroup?.Total ?? 0
                    },
                    NewGroup = new
                    {
                        Red = newGroup?.Red ?? 0,
                        Blue = newGroup?.Blue ?? 0,
                        Green = newGroup?.Green ?? 0,
                        White = newGroup?.White ?? 0,
                        Total = newGroup?.Total ?? 0
                    },
                    Differences = new
                    {
                        Red = (newGroup?.Red ?? 0) - (oldGroup?.Red ?? 0),
                        Blue = (newGroup?.Blue ?? 0) - (oldGroup?.Blue ?? 0),
                        Green = (newGroup?.Green ?? 0) - (oldGroup?.Green ?? 0),
                        White = (newGroup?.White ?? 0) - (oldGroup?.White ?? 0),
                        Total = (newGroup?.Total ?? 0) - (oldGroup?.Total ?? 0)
                    }
                };

                var json = JsonSerializer.Serialize(manaData);
                networkClient.SendRequest("ManaChange", json);

                Plugin.Logger?.LogInfo($"[EnergySync] Mana group changed: {changeReason}, " +
                    $"Old: R{oldGroup?.Red}B{oldGroup?.Blue}G{oldGroup?.Green}W{oldGroup?.White}, " +
                    $"New: R{newGroup?.Red}B{newGroup?.Blue}G{newGroup?.Green}W{newGroup?.White}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in SyncManaGroupChange: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 能量回复同步
    /// TODO: 找到游戏中所有能量回复的点
    /// 1. 回合开始时回复
    /// 2. 卡牌效果回复（如回能牌）
    /// 3. 宝物效果回复
    /// 4. 事件回复
    /// </summary>
    public class ManaRegainPatch
    {
        // TODO: Patch回合开始时的回能
        // [HarmonyPatch(typeof(TurnManager), "OnTurnStart")] 或其他回合开始方法
        // public static void Postfix(...) { }

        // TODO: Patch卡牌效果导致的回能
        // 需要找到GainManaAction或其他回能相关类

        // TODO: Patch宝物效果导致的回能
        // 需要在宝物触发逻辑中添加Patch

        /// <summary>
        /// 通用能量回复同步方法
        /// </summary>
        public static void SyncManaRegain(int amount, string source, string regainType)
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

                var regainData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    Amount = amount,
                    Source = source,
                    RegainType = regainType  // TurnStart/CardEffect/RelicEffect/Event
                };

                var json = JsonSerializer.Serialize(regainData);
                networkClient.SendRequest("ManaRegain", json);

                Plugin.Logger?.LogInfo($"[EnergySync] Mana regained: +{amount} from {source} ({regainType})");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in SyncManaRegain: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 能量消耗同步
    /// TODO: 同步卡牌使用时消耗的能量
    /// 需要捕获每个法力颜色的消耗
    /// </summary>
    public class ManaSpendPatch
    {
        // TODO: Patch Card.GetModifiedManaGroup 或卡牌使用时的消耗计算
        // 需要记录消耗前的法力状态和消耗后的状态

        /// <summary>
        /// 同步能量消耗
        /// </summary>
        public static void SyncManaSpend(ManaGroup cost, string cardId, string cardName)
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

                var spendData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    CardId = cardId,
                    CardName = cardName,
                    Cost = new
                    {
                        Red = cost?.Red ?? 0,
                        Blue = cost?.Blue ?? 0,
                        Green = cost?.Green ?? 0,
                        White = cost?.White ?? 0,
                        Total = cost?.Total ?? 0
                    }
                };

                var json = JsonSerializer.Serialize(spendData);
                networkClient.SendRequest("ManaSpend", json);

                Plugin.Logger?.LogInfo($"[EnergySync] Mana spent for {cardName}: R{cost?.Red}B{cost?.Blue}G{cost?.Green}W{cost?.White}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in SyncManaSpend: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 能量上限变更同步
    /// TODO: 找到能量上限变更的Patch点
    /// 如宝物、卡牌效果提升/降低能量上限
    /// </summary>
    public class MaxManaPatch
    {
        // TODO: Patch能量上限增加
        // 宝物效果：如某些宝物提供+1能量上限
        // 卡牌效果：如某些卡牌临时提升能量上限
        // 事件效果：事件选择导致的能量上限变化

        /// <summary>
        /// 同步能量上限变更
        /// </summary>
        public static void SyncMaxManaChange(int oldMax, int newMax, string source, string changeType)
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

                var changeData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    OldMaxMana = oldMax,
                    NewMaxMana = newMax,
                    Difference = newMax - oldMax,
                    Source = source,
                    ChangeType = changeType  // Permanent/Temporary
                };

                var json = JsonSerializer.Serialize(changeData);
                networkClient.SendRequest("MaxManaChange", json);

                Plugin.Logger?.LogInfo($"[EnergySync] Max mana changed: {oldMax} -> {newMax} by {source} ({changeType})");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in SyncMaxManaChange: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 回合开始时能量重置同步
    /// 每个回合开始时，能量会被重置为初始值
    /// </summary>
    public class TurnStartManaResetPatch
    {
        // TODO: Patch回合开始时的能量重置逻辑
        // 需要找到TurnManager或BattleController中的回合开始方法

        /// <summary>
        /// 同步回合开始时的能量状态
        /// </summary>
        public static void SyncTurnStartMana(ManaGroup startingMana)
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

                var resetData = new
                {
                    Timestamp = DateTime.Now.Ticks,
                    StartingMana = new
                    {
                        Red = startingMana?.Red ?? 0,
                        Blue = startingMana?.Blue ?? 0,
                        Green = startingMana?.Green ?? 0,
                        White = startingMana?.White ?? 0
                    }
                };

                var json = JsonSerializer.Serialize(resetData);
                networkClient.SendRequest("TurnStartMana", json);

                Plugin.Logger?.LogInfo($"[EnergySync] Turn start mana reset: R{startingMana?.Red}B{startingMana?.Blue}G{startingMana?.Green}W{startingMana?.White}");
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[EnergySync] Error in SyncTurnStartMana: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 构建完整的能量状态快照
    /// 用于断线重连或完整状态同步
    /// </summary>
    public static object BuildFullManaStateSnapshot(BattleController battle)
    {
        if (battle?.BattleMana == null)
        {
            return new { Error = "Battle or BattleMana is null" };
        }

        var mana = battle.BattleMana;

        return new
        {
            CurrentMana = new
            {
                Red = mana.Red,
                Blue = mana.Blue,
                Green = mana.Green,
                White = mana.White,
                Total = mana.Total
            },
            // TODO: 添加最大能量
            // MaxMana = ...,
            Timestamp = DateTime.Now.Ticks,
            BattleId = battle.GetHashCode().ToString()
        };
    }
}
