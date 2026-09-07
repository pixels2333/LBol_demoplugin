using System;
using LBoL.Base;

namespace NetworkPlugin.Utils
{
        public static class ManaUtils
    {
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
