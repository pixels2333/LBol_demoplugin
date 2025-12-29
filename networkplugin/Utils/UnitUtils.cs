using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Units;
using NetworkPlugin;

namespace NetworkPlugin.Utils;

/// <summary>
/// 单位工具类：提供对 LBoL 单位状态的轻量读取/快照。
/// <para/>
/// 说明：该仓库里曾存在一份不完整/拼接错误的版本导致编译失败；这里提供一个最小可用实现，
/// 以便网络同步/调试时能安全地读取常用字段（HP/格挡/护盾/存活等）。
/// </summary>
public static class UnitUtils
{
    /// <summary>
    /// 获取单位基础状态（适用于玩家/敌人等所有 Unit）。
    /// </summary>
    public static object GetUnitStatus(Unit unit)
    {
        if (unit == null)
        {
            return null;
        }

        try
        {
            return new
            {
                unit.Id,
                unit.Name,
                Type = unit.GetType().Name,
                unit.Hp,
                unit.MaxHp,
                unit.Block,
                unit.Shield,
                unit.IsAlive,
                Timestamp = DateTime.Now.Ticks,
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[UnitUtils] GetUnitStatus 失败: {ex.Message}");
            return new { Error = "GetUnitStatus failed" };
        }
    }

    /// <summary>
    /// 获取玩家单位的状态快照（在基础状态之上补充常见玩家字段，使用反射避免版本差异）。
    /// </summary>
    public static object GetPlayerStatus(PlayerUnit player)
    {
        if (player == null)
        {
            return null;
        }

        try
        {
            object basic = GetUnitStatus(player);

            int? power = TryGetIntProperty(player, "Power");
            int? maxPower = TryGetIntProperty(player, "MaxPower");

            return new
            {
                Basic = basic,
                Power = power,
                MaxPower = maxPower,
            };
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogError($"[UnitUtils] GetPlayerStatus 失败: {ex.Message}");
            return new { Error = "GetPlayerStatus failed" };
        }
    }

    /// <summary>
    /// 获取敌人单位的状态快照（当前仅返回基础状态）。
    /// </summary>
    public static object GetEnemyStatus(EnemyUnit enemy)
    {
        return GetUnitStatus(enemy);
    }

    /// <summary>
    /// 提取单位的状态效果名称列表（如果无法读取则返回空列表）。
    /// </summary>
    public static List<string> GetStatusEffects(Unit unit)
    {
        if (unit == null)
        {
            return [];
        }

        try
        {
            // 不同版本字段/属性可能不同，这里用反射尽量兼容：
            // - StatusEffects (IEnumerable)
            // - _statusEffects (私有字段)
            object se = TryGetProperty(unit, "StatusEffects") ?? TryGetField(unit, "_statusEffects");
            if (se is System.Collections.IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Select(x => x?.ToString() ?? string.Empty).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger?.LogDebug($"[UnitUtils] GetStatusEffects 失败: {ex.Message}");
        }

        return [];
    }

    private static int? TryGetIntProperty(object obj, string propertyName)
    {
        try
        {
            var p = obj.GetType().GetProperty(propertyName);
            if (p == null)
            {
                return null;
            }

            object v = p.GetValue(obj);
            return v == null ? null : Convert.ToInt32(v);
        }
        catch
        {
            return null;
        }
    }

    private static object TryGetProperty(object obj, string propertyName)
    {
        try
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }

    private static object TryGetField(object obj, string fieldName)
    {
        try
        {
            return obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.GetValue(obj);
        }
        catch
        {
            return null;
        }
    }
}
