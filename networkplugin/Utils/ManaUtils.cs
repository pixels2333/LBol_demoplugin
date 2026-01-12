using System;
using LBoL.Base;

namespace NetworkPlugin.Utils
{
    /// <summary>
    /// LBoL 法力系统工具类：用于处理 ManaGroup 的转换与简单计算。
    /// </summary>
    public static class ManaUtils
    {
        /// <summary>
        /// 将 ManaGroup 转换为数组格式 [红, 蓝, 绿, 白]。
        /// </summary>
        public static int[] ManaGroupToArray(ManaGroup manaGroup)
        {
            return
            [
                manaGroup.Red,
                manaGroup.Blue,
                manaGroup.Green,
                manaGroup.White
            ];
        }

        /// <summary>
        /// 将数组转换为 ManaGroup（数组格式 [红, 蓝, 绿, 白]）。
        /// </summary>
        public static ManaGroup ArrayToManaGroup(int[] manaArray)
        {
            if (manaArray == null || manaArray.Length < 4)
            {
                return ManaGroup.Empty;
            }

            return new ManaGroup
            {
                Red = manaArray[0],
                Blue = manaArray[1],
                Green = manaArray[2],
                White = manaArray[3],
            };
        }

        public static string ManaGroupToString(ManaGroup manaGroup)
        {
            return $"R{manaGroup.Red}B{manaGroup.Blue}G{manaGroup.Green}W{manaGroup.White}";
        }

        public static ManaGroup StringToManaGroup(string manaString)
        {
            try
            {
                if (string.IsNullOrEmpty(manaString))
                {
                    return ManaGroup.Empty;
                }

                string[] parts = manaString.Split(['R', 'B', 'G', 'W'], StringSplitOptions.RemoveEmptyEntries);

                int red = 0, blue = 0, green = 0, white = 0;

                if (parts.Length > 0)
                {
                    int.TryParse(parts[0], out red);
                }

                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out blue);
                }

                if (parts.Length > 2)
                {
                    int.TryParse(parts[2], out green);
                }

                if (parts.Length > 3)
                {
                    int.TryParse(parts[3], out white);
                }

                return new ManaGroup
                {
                    Red = red,
                    Blue = blue,
                    Green = green,
                    White = white,
                };
            }
            catch (Exception ex)
            {
                Plugin.Logger?.LogError($"[ManaUtils] Error parsing mana string '{manaString}': {ex.Message}");
                return ManaGroup.Empty;
            }
        }

        public static ManaGroup CalculateManaDifference(ManaGroup from, ManaGroup to)
        {
            return new ManaGroup
            {
                Red = to.Red - from.Red,
                Blue = to.Blue - from.Blue,
                Green = to.Green - from.Green,
                White = to.White - from.White,
            };
        }

        public static bool CanAffordMana(ManaGroup available, ManaGroup cost)
        {
            return available.Red >= cost.Red &&
                   available.Blue >= cost.Blue &&
                   available.Green >= cost.Green &&
                   available.White >= cost.White;
        }

        public static int GetTotalMana(ManaGroup manaGroup)
        {
            return manaGroup.Any +
                   manaGroup.White +
                   manaGroup.Blue +
                   manaGroup.Black +
                   manaGroup.Red +
                   manaGroup.Green +
                   manaGroup.Colorless +
                   manaGroup.Philosophy +
                   manaGroup.Hybrid;
        }

        public static ManaGroup GetEmptyManaGroup()
        {
            return ManaGroup.Empty;
        }

        public static ManaGroup CloneManaGroup(ManaGroup original)
        {
            return new ManaGroup
            {
                Any = original.Any,
                White = original.White,
                Blue = original.Blue,
                Black = original.Black,
                Red = original.Red,
                Green = original.Green,
                Colorless = original.Colorless,
                Philosophy = original.Philosophy,
                Hybrid = original.Hybrid,
                HybridColor = original.HybridColor,
            };
        }
    }
}
