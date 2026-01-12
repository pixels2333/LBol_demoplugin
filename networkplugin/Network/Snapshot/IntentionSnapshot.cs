using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetworkPlugin.Network.Snapshot;

/// <summary>
/// 敌人意图快照
/// </summary>
public class IntentionSnapshot
{

    /// <summary>
    /// 意图类型（攻击/防御/特殊等）
    /// </summary>
    public string IntentionType { get; set; }

    /// <summary>
    /// 意图名称
    /// </summary>
    public string IntentionName { get; set; }

    /// <summary>
    /// 意图值（伤害量/格挡量等）
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 意图描述
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 完整意图数据字典（按敌人分组）
    /// 格式：{ 敌人名称: [ {Type, Name, Value}, ... ] }
    /// </summary>
    public Dictionary<string, List<Dictionary<string, object>>> EnemyIntentions { get; set; }

    /// <summary>
    /// 简单意图构造函数（单意图）
    /// </summary>
    public IntentionSnapshot(string intentionType, string intentionName, int value)
    {
        IntentionType = intentionType;
        IntentionName = intentionName;
        Value = value;
    }

    /// <summary>
    /// 完整意图构造函数（多敌人多意图）
    /// 根据战斗中的所有存活敌人构建意图快照
    /// </summary>
    public IntentionSnapshot(object battleController)
    {
        if (battleController == null)
        {
            EnemyIntentions = new Dictionary<string, List<Dictionary<string, object>>>();
            return;
        }

        try
        {
            dynamic battle = battleController;
            var enemies = battle.AllAliveEnemies as IEnumerable<dynamic>;

            if (enemies == null)
            {
                EnemyIntentions = new Dictionary<string, List<Dictionary<string, object>>>();
                return;
            }

            EnemyIntentions = new Dictionary<string, List<Dictionary<string, object>>>();
            foreach (dynamic enemy in enemies)
            {
                string enemyName = enemy.Name;
                var intentions = new List<Dictionary<string, object>>();

                foreach (dynamic intention in enemy.Intentions)
                {
                    intentions.Add(GetIntentionDetails(intention));
                }

                EnemyIntentions[enemyName] = intentions;
            }
        }
        catch (Exception)
        {
            EnemyIntentions = new Dictionary<string, List<Dictionary<string, object>>>();
        }
    }

    /// <summary>
    /// 获取意图的详细信息
    /// 根据意图类型提取不同的属性（伤害、次数、倒计时等）
    /// </summary>
    private static Dictionary<string, object> GetIntentionDetails(dynamic intention)
    {
        var details = new Dictionary<string, object>
        {
            ["Type"] = intention.Type.ToString(),
            ["Name"] = intention.Name,
            ["Description"] = intention.Description ?? ""
        };

        // 根据意图类型提取特定属性
        string intentionType = ((object)intention).GetType().Name;

        try
        {
            switch (intentionType)
            {
                case "AttackIntention":
                    // 攻击意图：包含伤害值、攻击次数、是否精准
                    try
                    {
                        var damageInfo = intention.Damage;
                        if (damageInfo != null)
                        {
                            details["Damage"] = (int)damageInfo.Damage;
                            details["IsAccuracy"] = (bool)damageInfo.IsAccuracy;
                            details["DamageType"] = damageInfo.DamageType.ToString();
                        }
                    }
                    catch { /* 忽略访问错误 */ }

                    // 获取攻击次数（可能不存在）
                    try
                    {
                        if (intention.Times != null)
                        {
                            details["Times"] = (int)intention.Times;
                        }
                        else
                        {
                            details["Times"] = 1;
                        }
                    }
                    catch { details["Times"] = 1; }
                    break;

                case "CountDownIntention":
                    // 倒计时意图：包含倒计时数值
                    try
                    {
                        if (intention.Counter != null)
                        {
                            details["Counter"] = (int)intention.Counter;
                        }
                    }
                    catch { /* 忽略访问错误 */ }
                    break;

                case "DefendIntention":
                    // 防御意图：包含格挡值
                    try
                    {
                        if (intention.Block != null)
                        {
                            details["Block"] = (int)intention.Block;
                        }
                    }
                    catch { /* 忽略访问错误 */ }
                    break;

                case "SpellCardIntention":
                    // 法术卡意图：可能包含伤害值
                    try
                    {
                        var spellDamageInfo = intention.Damage;
                        if (spellDamageInfo != null)
                        {
                            details["Damage"] = (int)spellDamageInfo.Damage;
                            details["IsAccuracy"] = (bool)spellDamageInfo.IsAccuracy;
                            details["DamageType"] = spellDamageInfo.DamageType.ToString();
                        }
                    }
                    catch { /* 忽略访问错误 */ }

                    // 获取攻击次数（可能不存在）
                    try
                    {
                        if (intention.Times != null)
                        {
                            details["Times"] = (int)intention.Times;
                        }
                    }
                    catch { /* 忽略访问错误 */ }

                    // 获取法术图标名称
                    try
                    {
                        if (!string.IsNullOrEmpty(intention.IconName))
                        {
                            details["IconName"] = intention.IconName;
                        }
                    }
                    catch { /* 忽略访问错误 */ }
                    break;

                case "HealIntention":
                    // 治疗意图
                    details["HealType"] = "Heal";
                    break;

                case "BuffIntention":
                    // 增益意图
                    details["BuffType"] = "Buff";
                    break;

                case "DebuffIntention":
                    // 减益意图
                    details["DebuffType"] = "Debuff";
                    break;

                default:
                    // 其他意图类型，记录类型名称以便调试
                    details["IntentionClass"] = intentionType;
                    break;
            }
        }
        catch (Exception ex)
        {
            details["Error"] = $"Failed to extract intention details: {ex.Message}";
        }

        return details;
    }
}
