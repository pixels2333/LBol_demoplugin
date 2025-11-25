using System;
using System.Linq;
namespace LBoL.Base
{
	public static class ManaGroupExtensions
	{
		public static ManaGroup With(this ManaGroup source, int? any = null, int? white = null, int? blue = null, int? black = null, int? red = null, int? green = null, int? colorless = null, int? philosophy = null)
		{
			return new ManaGroup
			{
				Any = (any ?? source.Any),
				White = (white ?? source.White),
				Blue = (blue ?? source.Blue),
				Black = (black ?? source.Black),
				Red = (red ?? source.Red),
				Green = (green ?? source.Green),
				Colorless = (colorless ?? source.Colorless),
				Philosophy = (philosophy ?? source.Philosophy)
			};
		}
		public static ManaGroup WithAny(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Any = value;
			return manaGroup;
		}
		public static ManaGroup WithWhite(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.White = value;
			return manaGroup;
		}
		public static ManaGroup WithBlue(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Blue = value;
			return manaGroup;
		}
		public static ManaGroup WithBlack(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Black = value;
			return manaGroup;
		}
		public static ManaGroup WithRed(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Red = value;
			return manaGroup;
		}
		public static ManaGroup WithGreen(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Green = value;
			return manaGroup;
		}
		public static ManaGroup WithColorless(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Colorless = value;
			return manaGroup;
		}
		public static ManaGroup WithPhilosophy(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Philosophy = value;
			return manaGroup;
		}
		public static ManaGroup Purified(this ManaGroup mana)
		{
			mana.White = 0;
			mana.Blue = 0;
			mana.Black = 0;
			mana.Red = 0;
			mana.Green = 0;
			mana.Hybrid = 0;
			return mana;
		}
		public static ManaGroup CostToMana(this ManaGroup mana)
		{
			mana.Colorless += mana.Any;
			mana.Any = 0;
			mana.Philosophy += mana.Hybrid;
			mana.Hybrid = 0;
			return mana;
		}
		public static bool IsSubset(this ManaGroup mana, ManaGroup other)
		{
			if (mana.Hybrid > 0)
			{
				return mana.HybridColor == other.HybridColor && Enumerable.All<ManaColor>(ManaColors.AllColorsWithHybrid, (ManaColor manaColor) => mana.GetValue(manaColor) <= other.GetValue(manaColor));
			}
			return Enumerable.All<ManaColor>(ManaColors.ColorsWithAny, (ManaColor manaColor) => mana.GetValue(manaColor) <= other.GetValue(manaColor));
		}
		public static bool IsSuperset(this ManaGroup mana, ManaGroup other)
		{
			if (other.Hybrid > 0)
			{
				return mana.HybridColor == other.HybridColor && Enumerable.All<ManaColor>(ManaColors.AllColorsWithHybrid, (ManaColor manaColor) => mana.GetValue(manaColor) >= other.GetValue(manaColor));
			}
			return Enumerable.All<ManaColor>(ManaColors.ColorsWithAny, (ManaColor manaColor) => mana.GetValue(manaColor) >= other.GetValue(manaColor));
		}
	}
}
