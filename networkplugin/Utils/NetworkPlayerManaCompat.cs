using System;
using NetworkPlugin.Network.NetworkPlayer;

namespace NetworkPlugin.Utils;

public static class NetworkPlayerManaCompat
{
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

        }
    }
}
