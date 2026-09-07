using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Units;

namespace NetworkPlugin.Utils;

public static class UnitUtils
{
        public static object GetUnitStatus(Unit unit)
    {
        if (unit == null)
        {
            return null;
        }

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

        public static object GetPlayerStatus(PlayerUnit player)
    {
        if (player == null)
        {
            return null;
        }

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

        public static object GetEnemyStatus(EnemyUnit enemy)
    {
        return GetUnitStatus(enemy);
    }

        public static List<string> GetStatusEffects(Unit unit)
    {
        if (unit == null)
        {
            return [];
        }

        try
        {

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
        var p = obj.GetType().GetProperty(propertyName);
        if (p == null)
        {
            return null;
        }

        object v = p.GetValue(obj);
        return v == null ? null : Convert.ToInt32(v);
    }

    private static object TryGetProperty(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
    }

    private static object TryGetField(object obj, string fieldName)
    {
        return obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)?.GetValue(obj);
    }
}
