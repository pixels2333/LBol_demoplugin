using System;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Utils;

/// <summary>
/// INetworkPlayer 的战斗资源兼容工具：
/// 当前字段可能不存在，因此统一使用反射做安全读取。
/// </summary>
public static class NetworkPlayerBattleStateCompat
{
    public static int GetCurrentPowerSafe(this INetworkPlayer player)
    {
        return GetIntProperty(player, "CurrentPower", "currentPower", "Power");
    }

    public static int GetPowerPerLevelSafe(this INetworkPlayer player)
    {
        return GetIntProperty(player, "PowerPerLevel", "powerPerLevel");
    }

    public static int GetMaxPowerLevelSafe(this INetworkPlayer player)
    {
        return GetIntProperty(player, "MaxPowerLevel", "maxPowerLevel");
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
}
