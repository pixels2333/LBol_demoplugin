using System;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Utils;

/// <summary>
/// INetworkPlayer 的战斗资源兼容工具：
/// - 当前符卡积攒能量/每段需求/最大段数仍挂在实现类上；
/// - 这里统一做反射读写，避免直接扩展接口破坏旧调用面。
/// </summary>
public static class NetworkPlayerBattleStateCompat
{
    public static int GetCurrentPowerSafe(this INetworkPlayer player)
    {
        return GetIntProperty(player, "CurrentPower", "currentPower", "Power");
    }

    public static void SetCurrentPowerSafe(this INetworkPlayer player, int value)
    {
        SetIntProperty(player, Math.Max(0, value), "CurrentPower", "currentPower", "Power");
    }

    public static int GetPowerPerLevelSafe(this INetworkPlayer player)
    {
        return GetIntProperty(player, "PowerPerLevel", "powerPerLevel");
    }

    public static void SetPowerPerLevelSafe(this INetworkPlayer player, int value)
    {
        SetIntProperty(player, Math.Max(0, value), "PowerPerLevel", "powerPerLevel");
    }

    public static int GetMaxPowerLevelSafe(this INetworkPlayer player)
    {
        return GetIntProperty(player, "MaxPowerLevel", "maxPowerLevel");
    }

    public static void SetMaxPowerLevelSafe(this INetworkPlayer player, int value)
    {
        SetIntProperty(player, Math.Max(0, value), "MaxPowerLevel", "maxPowerLevel");
    }

    private static int GetIntProperty(INetworkPlayer player, params string[] propertyNames)
    {
        try
        {
            if (player == null)
            {
                return 0;
            }

            Type type = player.GetType();
            foreach (string propertyName in propertyNames)
            {
                var property = type.GetProperty(propertyName);
                if (property == null)
                {
                    continue;
                }

                object value = property.GetValue(player);
                if (value is int intValue)
                {
                    return intValue;
                }
            }
        }
        catch
        {
            // ignored
        }

        return 0;
    }

    private static void SetIntProperty(INetworkPlayer player, int value, params string[] propertyNames)
    {
        try
        {
            if (player == null)
            {
                return;
            }

            Type type = player.GetType();
            foreach (string propertyName in propertyNames)
            {
                var property = type.GetProperty(propertyName);
                if (property == null || property.PropertyType != typeof(int) || !property.CanWrite)
                {
                    continue;
                }

                property.SetValue(player, value);
                return;
            }
        }
        catch
        {
            // ignored
        }
    }
}