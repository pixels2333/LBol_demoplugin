using System;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Utils;

/// <summary>
/// INetworkPlayer 的历史兼容工具：
/// - 一些代码路径希望访问实现类上的 lowerCamelCase 属性（例如 mana），但接口未声明；
/// - 这里用反射读取/写入，避免扩展接口导致的破坏性修改。
/// </summary>
public static class NetworkPlayerManaCompat
{
    /// <summary>
    /// 获取玩家法力数组，保证返回长度为 4 的数组（不足补 0，超过截断）。
    /// </summary>
    public static int[] GetManaArraySafe(this INetworkPlayer player)
    {
        try
        {
            if (player == null)
            {
                return [0, 0, 0, 0];
            }

            var t = player.GetType();
            var prop = t.GetProperty("mana") ?? t.GetProperty("Mana");
            if (prop != null && prop.PropertyType == typeof(int[]))
            {
                var v = prop.GetValue(player) as int[];
                if (v == null)
                {
                    return [0, 0, 0, 0];
                }

                if (v.Length == 4)
                {
                    return v;
                }

                int[] fixedMana = [0, 0, 0, 0];
                for (int i = 0; i < Math.Min(4, v.Length); i++)
                {
                    fixedMana[i] = v[i];
                }

                return fixedMana;
            }

            return [0, 0, 0, 0];
        }
        catch
        {
            return [0, 0, 0, 0];
        }
    }

    /// <summary>
    /// 尝试写入玩家法力数组（若实现类暴露 mana/Mana 属性）。
    /// </summary>
    public static void SetManaArraySafe(this INetworkPlayer player, int[] mana)
    {
        try
        {
            if (player == null)
            {
                return;
            }

            int[] v = mana ?? [0, 0, 0, 0];
            if (v.Length != 4)
            {
                int[] fixedMana = [0, 0, 0, 0];
                for (int i = 0; i < Math.Min(4, v.Length); i++)
                {
                    fixedMana[i] = v[i];
                }
                v = fixedMana;
            }

            var t = player.GetType();
            var prop = t.GetProperty("mana") ?? t.GetProperty("Mana");
            if (prop != null && prop.PropertyType == typeof(int[]))
            {
                prop.SetValue(player, v);
            }
        }
        catch
        {
            // ignored
        }
    }
}
