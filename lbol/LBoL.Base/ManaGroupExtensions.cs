using System;
using System.Linq;

namespace LBoL.Base
{
	// Token: 0x02000014 RID: 20
	public static class ManaGroupExtensions
	{
		// Token: 0x06000087 RID: 135 RVA: 0x00004424 File Offset: 0x00002624
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

		// Token: 0x06000088 RID: 136 RVA: 0x00004550 File Offset: 0x00002750
		public static ManaGroup WithAny(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Any = value;
			return manaGroup;
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00004568 File Offset: 0x00002768
		public static ManaGroup WithWhite(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.White = value;
			return manaGroup;
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00004580 File Offset: 0x00002780
		public static ManaGroup WithBlue(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Blue = value;
			return manaGroup;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00004598 File Offset: 0x00002798
		public static ManaGroup WithBlack(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Black = value;
			return manaGroup;
		}

		// Token: 0x0600008C RID: 140 RVA: 0x000045B0 File Offset: 0x000027B0
		public static ManaGroup WithRed(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Red = value;
			return manaGroup;
		}

		// Token: 0x0600008D RID: 141 RVA: 0x000045C8 File Offset: 0x000027C8
		public static ManaGroup WithGreen(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Green = value;
			return manaGroup;
		}

		// Token: 0x0600008E RID: 142 RVA: 0x000045E0 File Offset: 0x000027E0
		public static ManaGroup WithColorless(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Colorless = value;
			return manaGroup;
		}

		// Token: 0x0600008F RID: 143 RVA: 0x000045F8 File Offset: 0x000027F8
		public static ManaGroup WithPhilosophy(this ManaGroup mana, int value)
		{
			ManaGroup manaGroup = mana;
			manaGroup.Philosophy = value;
			return manaGroup;
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00004610 File Offset: 0x00002810
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

		// Token: 0x06000091 RID: 145 RVA: 0x00004643 File Offset: 0x00002843
		public static ManaGroup CostToMana(this ManaGroup mana)
		{
			mana.Colorless += mana.Any;
			mana.Any = 0;
			mana.Philosophy += mana.Hybrid;
			mana.Hybrid = 0;
			return mana;
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00004680 File Offset: 0x00002880
		public static bool IsSubset(this ManaGroup mana, ManaGroup other)
		{
			if (mana.Hybrid > 0)
			{
				return mana.HybridColor == other.HybridColor && Enumerable.All<ManaColor>(ManaColors.AllColorsWithHybrid, (ManaColor manaColor) => mana.GetValue(manaColor) <= other.GetValue(manaColor));
			}
			return Enumerable.All<ManaColor>(ManaColors.ColorsWithAny, (ManaColor manaColor) => mana.GetValue(manaColor) <= other.GetValue(manaColor));
		}

		// Token: 0x06000093 RID: 147 RVA: 0x000046F8 File Offset: 0x000028F8
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
