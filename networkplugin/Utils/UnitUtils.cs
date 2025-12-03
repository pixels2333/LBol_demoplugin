using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// LBoL单位工具类 - 用于处理玩家和敌人的状态信息
    /// </summary>
    public static class UnitUtils
    {
        /// <summary>
        /// 获取单位的基本状态信息
        /// </summary>
        /// <param name="unit">游戏单位</param>
        /// <returns>单位状态信息</returns>
        public static object GetUnitStatus(GameUnit unit)
        {
            if (unit == null)
            {
                return null;
            }

            try
            {
                var isPlayer = unit is PlayerUnit;
                var isEnemy = unit is EnemyUnit;

                return new
                {
                    Id = unit.Id,
                    Name = unit.Name,
                    Type = unit.GetType().Name,
                    IsPlayer = isPlayer,
                    IsEnemy = isEnemy,
                    Hp = unit.Hp,
                    MaxHp = unit.MaxHp,
                    Block = unit.Block,
                    Shield = unit.Shield,
                    IsAlive = unit.IsAlive,
                    // TODO: 添加更多状态效果和属性
                    StatusEffects = GetStatusEffects(unit),
                    Timestamp = DateTime.Now.Ticks
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[UnitUtils] Error getting unit status: {ex.Message}");
                return new { Error = "Failed to get unit status" };
            }
        }

        /// <summary>
        /// 获取玩家的详细状态
        /// </summary>
        /// <param name="player">玩家单位</param>
        /// <returns>玩家状态信息</returns>
        public static object GetPlayerStatus(PlayerUnit player)
        {
            if (player == null)
            {
                return null;
            }

            try
            {
                dynamic basicStatus = GetUnitStatus(player) as dynamic;

                return new
                {
                    // 基本信息继承自GetUnitStatus
                    BasicInfo = basicStatus,

                    // 玩家特有信息
                    Power = GetPlayerPower(player),
                    MaxPower = GetPlayerMaxPower(player),
                    Mana = GetCurrentMana(player),
                    CardInfo = CardUtils.GetCardZoneInfo(player),
                    // TODO: 添加更多玩家特有属性
                    // Exhibits = GetExhibits(player),
                    // HandSizeLimit = GetHandSizeLimit(player),

                    Timestamp = DateTime.Now.Ticks
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[UnitUtils] Error getting player status: {ex.Message}");
                return new { Error = "Failed to get player status" };
            }
        }

        /// <summary>
        /// 获取敌人的详细状态
        /// </summary>
        /// <param name="enemy">敌人单位</param>
        /// <returns>敌人状态信息</returns>
        public static object GetEnemyStatus(EnemyUnit enemy)
        {
            if (enemy == null)
            {
                return null;
            }

            try
            {
                dynamic basicStatus = GetUnitStatus(enemy) as dynamic;

                return new
                {
                    // 基本信息继承自GetUnitStatus
                    BasicInfo = basicStatus,

                    // 敌人特有信息
                    EnemyId = enemy.Config?.Id ?? "Unknown",
                    Difficulty = enemy.Config?.Difficulty ?? 0,
                    AiPattern = enemy.Config?.AiPattern.ToString() ?? "Unknown",
                    Intentions = GetCurrentIntentions(enemy),
                    // TODO: 添加更多敌人特有属性
                    // RewardInfo = GetBattleRewards(enemy),

                    Timestamp = DateTime.Now.Ticks
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[UnitUtils] Error getting enemy status: {ex.Message}");
                return new { Error = "Failed to get enemy status" };
            }
        }

        /// <summary>
        /// 获取单位的状态效果列表
        /// </summary>
        /// <param name="unit">游戏单位</param>
        /// <returns>状态效果列表</returns>
        public static List<object> GetStatusEffects(GameUnit unit)
        {
            List<object> statusEffects = [];

            try
            {
                // 尝试获取状态效果列表
                var statusEffectsProperty = unit.GetType().GetProperty("StatusEffects");
                if (statusEffectsProperty != null)
                {
                    IEnumerable effects = statusEffectsProperty.GetValue(unit) as System.Collections.IEnumerable;
                    if (effects != null)
                    {
                        foreach (var effect in effects)
                        {
                            if (effect != null)
                            {
                                var statusEffectInfo = new
                                {
                                    Id = GetStatusEffectId(effect),
                                    Name = GetStatusEffectName(effect),
                                    Level = GetStatusEffectLevel(effect),
                                    Duration = GetStatusEffectDuration(effect),
                                    IsActive = GetStatusEffectActive(effect),
                                    Type = effect.GetType().Name,
                                    Timestamp = DateTime.Now.Ticks
                                };
                                statusEffects.Add(statusEffectInfo);
                            }
                        }
                    }
                }

                // 备用方案：检查其他可能的状态效果属性
                var alternativeProperties = new[] { "Buffs", "Debuffs", "Effects", "ActiveEffects" };
                foreach (var propName in alternativeProperties)
                {
                    var prop = unit.GetType().GetProperty(propName);
                    if (prop != null)
                    {
                        IEnumerable effects = prop.GetValue(unit) as System.Collections.IEnumerable;
                        if (effects != null)
                        {
                            foreach (var effect in effects)
                            {
                                if (effect != null && !statusEffects.Exists(e => ((dynamic)e).Id == GetStatusEffectId(effect)))
                                {
                                    var statusEffectInfo = new
                                    {
                                        Id = GetStatusEffectId(effect),
                                        Name = GetStatusEffectName(effect),
                                        Level = GetStatusEffectLevel(effect),
                                        Duration = GetStatusEffectDuration(effect),
                                        IsActive = GetStatusEffectActive(effect),
                                        Type = effect.GetType().Name,
                                        Timestamp = DateTime.Now.Ticks
                                    };
                                    statusEffects.Add(statusEffectInfo);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[UnitUtils] Error getting status effects: {ex.Message}");
            }

            return statusEffects;
        }

        /// <summary>
        /// 获取玩家的当前能量
        /// </summary>
        /// <param name="player">玩家单位</param>
        /// <returns>当前能量值</returns>
        public static int GetPlayerPower(PlayerUnit player)
        {
            try
            {
                // 使用GameStateUtils中的实现
                return GameStateUtils.GetPlayerPower(player);
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[UnitUtils] Error getting player power: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 获取玩家的最大能量
        /// </summary>
        /// <param name="player">玩家单位</param>
        /// <returns>最大能量值</returns>
        public static int GetPlayerMaxPower(PlayerUnit player)
        {
            try
            {
                if (player == null)
                {
                    return 0;
                }

                // 尝试获取最大能量属性
                var maxPowerProperties = new[] { "MaxPower", "MaxEnergy", "PowerLimit", "EnergyLimit" };
                foreach (var propName in maxPowerProperties)
                {
                    var prop = player.GetType().GetProperty(propName);
                    if (prop != null && prop.PropertyType == typeof(int))
                    {
                        return (int)(prop.GetValue(player) ?? 0);
                    }
                }

                // 备用方案：从游戏运行状态获取
                var gameRun = GameStateUtils.GetCurrentGameRun();
                if (gameRun != null)
                {
                    var maxManaProperty = gameRun.GetType().GetProperty("MaxMana");
                    if (maxManaProperty != null)
                    {
                        return (int?)(maxManaProperty.GetValue(gameRun)) ?? 0;
                    }
                }

                return 3; // 默认最大能量
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[UnitUtils] Error getting player max power: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// 获取玩家的当前法力
        /// </summary>
        /// <param name="player">玩家单位</param>
        /// <returns>当前法力数组</returns>
        public static int[] GetCurrentMana(PlayerUnit player)
        {
            try
            {
                // 使用GameStateUtils中的实现
                var manaGroup = GameStateUtils.GetCurrentMana(player);

                // 转换为数组格式
                if (manaGroup != null)
                {
                    var redProperty = manaGroup.GetType().GetProperty("Red");
                    var blueProperty = manaGroup.GetType().GetProperty("Blue");
                    var greenProperty = manaGroup.GetType().GetProperty("Green");
                    var whiteProperty = manaGroup.GetType().GetProperty("White");

                    if (redProperty != null && blueProperty != null && greenProperty != null && whiteProperty != null)
                    {
                        return
                        [
                            (int)(redProperty.GetValue(manaGroup) ?? 0),
                            (int)(blueProperty.GetValue(manaGroup) ?? 0),
                            (int)(greenProperty.GetValue(manaGroup) ?? 0),
                            (int)(whiteProperty.GetValue(manaGroup) ?? 0)
                        ];
                    }
                }

                return [0, 0, 0, 0];
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[UnitUtils] Error getting player current mana: {ex.Message}");
                return [0, 0, 0, 0];
            }
        }

        /// <summary>
        /// 获取敌人的当前意图
        /// </summary>
        /// <param name="enemy">敌人单位</param>
        /// <returns>意图信息</returns>
        public static object GetCurrentIntentions(EnemyUnit enemy)
        {
            try
            {
                if (enemy == null)
                {
                    return new { IntentionType = "Unknown", Description = "No enemy" };
                }

                // 尝试获取意图信息
                var intentionProperty = enemy.GetType().GetProperty("Intention");
                if (intentionProperty != null)
                {
                    var intention = intentionProperty.GetValue(enemy);
                    if (intention != null)
                    {
                        return new
                        {
                            IntentionType = intention.GetType().Name,
                            Description = intention.ToString(),
                            IsAttacking = CheckIfIntentionIsAttack(intention),
                            IsDefending = CheckIfIntentionIsDefense(intention),
                            Target = GetIntentionTarget(intention),
                            Value = GetIntentionValue(intention)
                        };
                    }
                }

                return new { IntentionType = "Unknown", Description = "No intention found" };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[UnitUtils] Error getting enemy intentions: {ex.Message}");
                return new { IntentionType = "Error", Description = ex.Message };
            }
        }

        // 状态效果辅助方法

        /// <summary>
        /// 获取状态效果ID
        /// </summary>
        private static string GetStatusEffectId(object statusEffect)
        {
            try
            {
                var idProperty = statusEffect.GetType().GetProperty("Id");
                return idProperty?.GetValue(statusEffect)?.ToString() ?? statusEffect.GetType().Name;
            }
            catch
            {
                return statusEffect.GetType().Name;
            }
        }

        /// <summary>
        /// 获取状态效果名称
        /// </summary>
        private static string GetStatusEffectName(object statusEffect)
        {
            try
            {
                var nameProperty = statusEffect.GetType().GetProperty("Name");
                if (nameProperty != null)
                {
                    return nameProperty.GetValue(statusEffect)?.ToString() ?? "Unknown";
                }

                // 尝试从配置获取名称
                var configProperty = statusEffect.GetType().GetProperty("Config");
                if (configProperty != null)
                {
                    var config = configProperty.GetValue(statusEffect);
                    if (config != null)
                    {
                        var nameConfigProperty = config.GetType().GetProperty("Name");
                        if (nameConfigProperty != null)
                        {
                            return nameConfigProperty.GetValue(config)?.ToString() ?? "Unknown";
                        }
                    }
                }

                return statusEffect.GetType().Name;
            }
            catch
            {
                return statusEffect.GetType().Name;
            }
        }

        /// <summary>
        /// 获取状态效果等级
        /// </summary>
        private static int GetStatusEffectLevel(object statusEffect)
        {
            try
            {
                var levelProperties = new[] { "Level", "Amount", "Stack", "Power" };
                foreach (var propName in levelProperties)
                {
                    var prop = statusEffect.GetType().GetProperty(propName);
                    if (prop != null && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float)))
                    {
                        var value = prop.GetValue(statusEffect);
                        if (value != null)
                        {
                            return Convert.ToInt32(value);
                        }
                    }
                }
                return 1;
            }
            catch
            {
                return 1;
            }
        }

      // 检查状态效果的等级属性，优先查找Level、Amount、Stack、Power等属性

        /// <summary>
        /// 获取状态效果持续时间
        /// </summary>
        private static int GetStatusEffectDuration(object statusEffect)
        {
            try
            {
                var durationProperties = new[] { "Duration", "TurnsLeft", "RemainingTurns", "TimeLeft" };
                foreach (var propName in durationProperties)
                {
                    var prop = statusEffect.GetType().GetProperty(propName);
                    if (prop != null && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float)))
                    {
                        var value = prop.GetValue(statusEffect);
                        if (value != null)
                        {
                            return Convert.ToInt32(value);
                        }
                    }
                }
                return -1; // -1表示永久效果
            }
            catch
            {
                return -1;
            }
        }

    // 检查状态效果的持续时间，优先查找Duration、TurnsLeft、RemainingTurns、TimeLeft等属性

        /// <summary>
        /// 检查状态效果是否激活
        /// </summary>
        private static bool GetStatusEffectActive(object statusEffect)
        {
            try
            {
                var activeProperties = new[] { "IsActive", "Active", "Enabled" };
                foreach (var propName in activeProperties)
                {
                    var prop = statusEffect.GetType().GetProperty(propName);
                    if (prop != null && prop.PropertyType == typeof(bool))
                    {
                        var value = prop.GetValue(statusEffect);
                        if (value != null)
                        {
                            return (bool)value;
                        }
                    }
                }
                return true; // 默认激活
            }
            catch
            {
                return true;
            }
        }

      // 检查状态效果的激活状态，优先查找IsActive、Active、Enabled等布尔属性

        /// <summary>
        /// 检查意图是否为攻击
        /// </summary>
        private static bool CheckIfIntentionIsAttack(object intention)
        {
            try
            {
                var typeName = intention.GetType().Name.ToLower();
                return typeName.Contains("attack") || typeName.Contains("damage") || typeName.Contains("offense");
            }
            catch
            {
                return false;
            }
        }

      // 通过类型名称判断是否为攻击意图，包含attack、damage、offense等关键词

        /// <summary>
        /// 检查意图是否为防御
        /// </summary>
        private static bool CheckIfIntentionIsDefense(object intention)
        {
            try
            {
                var typeName = intention.GetType().Name.ToLower();
                return typeName.Contains("defend") || typeName.Contains("block") || typeName.Contains("guard");
            }
            catch
            {
                return false;
            }
        }

    // 通过类型名称判断是否为防御意图，包含defend、block、guard等关键词

        /// <summary>
        /// 获取意图目标
        /// </summary>
        private static string GetIntentionTarget(object intention)
        {
            try
            {
                var targetProperty = intention.GetType().GetProperty("Target");
                if (targetProperty != null)
                {
                    return targetProperty.GetValue(intention)?.ToString() ?? "Unknown";
                }

                var typeName = intention.GetType().Name.ToLower();
                if (typeName.Contains("self"))
                {
                    return "Self";
                }

                if (typeName.Contains("player"))
                {
                    return "Player";
                }

                if (typeName.Contains("random"))
                {
                    return "Random";
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

    // 根据意图对象获取目标，优先查找Target属性，否则根据类型名称推断目标类型

        /// <summary>
        /// 获取意图数值
        /// </summary>
        private static int GetIntentionValue(object intention)
        {
            try
            {
                var valueProperties = new[] { "Damage", "Block", "Amount", "Value", "Power" };
                foreach (var propName in valueProperties)
                {
                    var prop = intention.GetType().GetProperty(propName);
                    if (prop != null && (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(float)))
                    {
                        var value = prop.GetValue(intention);
                        if (value != null)
                        {
                            return Convert.ToInt32(value);
                        }
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

  // 从意图对象中获取数值，优先查找Damage、Block、Amount、Value、Power等属性
    }
}
}
                return new int[4] { 0, 0, 0, 0 };
            }
        }

        /// <summary>
        /// 获取敌人的当前意图
        /// </summary>
        /// <param name="enemy">敌人单位</param>
        /// <returns>意图列表</returns>
        public static List<object> GetCurrentIntentions(EnemyUnit enemy)
{
    List<object> intentions = [];

    try
    {
        // TODO: 实现获取敌人意图的逻辑
        // 敌人意图通常包含接下来要执行的动作
    }
    catch (Exception ex)
    {
        Plugin.Logger?.LogError($"[UnitUtils] Error getting current intentions: {ex.Message}");
    }

    return intentions;
}

/// <summary>
/// 创建单位的完整状态快照
/// </summary>
/// <param name="unit">游戏单位</param>
/// <returns>完整状态快照</returns>
public static object CreateUnitSnapshot(GameUnit unit)
{
    if (unit == null)
    {
        return null;
    }

    try
    {
        if (unit is PlayerUnit player)
        {
            return GetPlayerStatus(player);
        }

        if (unit is EnemyUnit enemy)
        {
            return GetEnemyStatus(enemy);
        }

        // 其他类型的单位
        return GetUnitStatus(unit);
    }
    catch (Exception ex)
    {
        Plugin.Logger?.LogError($"[UnitUtils] Error creating unit snapshot: {ex.Message}");
        return new { Error = "Failed to create unit snapshot" };
    }
}

/// <summary>
/// 检查单位是否受到特定状态效果
/// </summary>
/// <param name="unit">游戏单位</param>
/// <param name="statusEffectName">状态效果名称</param>
/// <returns>如果受到该状态效果返回true</returns>
public static bool HasStatusEffect(GameUnit unit, string statusEffectName)
{
    try
    {
        // TODO: 实现检查状态效果的逻辑
        return false;
    }
    catch (Exception ex)
    {
        Plugin.Logger?.LogError($"[UnitUtils] Error checking status effect: {ex.Message}");
        return false;
    }
}
    }
}