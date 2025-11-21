using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;

namespace LBoL.Base
{
	// Token: 0x02000010 RID: 16
	public static class ManaColors
	{
		// Token: 0x06000024 RID: 36 RVA: 0x000025C0 File Offset: 0x000007C0
		private static ManaColor? TryGetByIndex(int index)
		{
			if (index >= 0 && index < ManaColors.ColorTable.Length)
			{
				return ManaColors.ColorTable[index];
			}
			return default(ManaColor?);
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000025F0 File Offset: 0x000007F0
		public static void GetLoopOrder(ManaColor firstIn, ManaColor secondIn, out ManaColor firstOut, out ManaColor secondOut)
		{
			if (firstIn == secondIn)
			{
				secondOut = firstIn;
				firstOut = firstIn;
				return;
			}
			if (firstIn < secondIn)
			{
				firstOut = firstIn;
				secondOut = secondIn;
			}
			else
			{
				firstOut = secondIn;
				secondOut = firstIn;
			}
			if ((firstOut == ManaColor.White && secondOut == ManaColor.Red) || (firstOut == ManaColor.White && secondOut == ManaColor.Green) || (firstOut == ManaColor.Blue && secondOut == ManaColor.Green))
			{
				ManaColor manaColor = secondOut;
				ManaColor manaColor2 = firstOut;
				firstOut = manaColor;
				secondOut = manaColor2;
			}
		}

		// Token: 0x06000026 RID: 38 RVA: 0x00002648 File Offset: 0x00000848
		public static ManaColor Parse(string value)
		{
			ManaColor? manaColor = ManaColors.TryGetByIndex(ManaColors.Names.IndexOf(value));
			if (manaColor == null)
			{
				throw new ArgumentException(value + " is not a valid ManaColor");
			}
			return manaColor.GetValueOrDefault();
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002688 File Offset: 0x00000888
		public static bool TryParse(string value, out ManaColor result)
		{
			ManaColor? manaColor = ManaColors.TryGetByIndex(ManaColors.Names.IndexOf(value));
			if (manaColor != null)
			{
				ManaColor valueOrDefault = manaColor.GetValueOrDefault();
				result = valueOrDefault;
				return true;
			}
			result = ManaColor.Any;
			return false;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x000026C0 File Offset: 0x000008C0
		public static char GetShortName(ManaColor color)
		{
			char? c = ManaColors.ShortNames[(int)color];
			if (c == null)
			{
				throw new ArgumentException(string.Format("Invalid {0}: {1}", "ManaColor", color));
			}
			return c.GetValueOrDefault();
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002704 File Offset: 0x00000904
		public static string GetLongName(ManaColor color)
		{
			string text = ManaColors.Names[(int)color];
			if (text == null)
			{
				throw new ArgumentException(string.Format("Invalid {0}: {1}", "ManaColor", color));
			}
			return text;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x0000272C File Offset: 0x0000092C
		public static ManaColor? FromShortName(char name)
		{
			int num = ManaColors.ShortNames.IndexOf(new char?(char.ToUpperInvariant(name)));
			if (num < 0)
			{
				return default(ManaColor?);
			}
			return ManaColors.ColorTable[num];
		}

		// Token: 0x0400007B RID: 123
		private static readonly string[] Names = new string[] { "Any", "White", "Blue", "Black", "Red", "Green", "Colorless", "Philosophy", "Hybrid" };

		// Token: 0x0400007C RID: 124
		private static readonly char?[] ShortNames = new char?[]
		{
			new char?('1'),
			new char?('W'),
			new char?('U'),
			new char?('B'),
			new char?('R'),
			new char?('G'),
			new char?('C'),
			new char?('P'),
			new char?('H')
		};

		// Token: 0x0400007D RID: 125
		private static readonly ManaColor?[] ColorTable = new ManaColor?[]
		{
			new ManaColor?(ManaColor.Any),
			new ManaColor?(ManaColor.White),
			new ManaColor?(ManaColor.Blue),
			new ManaColor?(ManaColor.Black),
			new ManaColor?(ManaColor.Red),
			new ManaColor?(ManaColor.Green),
			new ManaColor?(ManaColor.Colorless),
			new ManaColor?(ManaColor.Philosophy),
			new ManaColor?(ManaColor.Hybrid)
		};

		// Token: 0x0400007E RID: 126
		public static readonly IReadOnlyList<ManaColor> WUBRG = new ManaColor[]
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green
		}.AsReadOnly<ManaColor>();

		// Token: 0x0400007F RID: 127
		public static readonly IReadOnlyList<ManaColor> WUBRGP = new ManaColor[]
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green,
			ManaColor.Philosophy
		}.AsReadOnly<ManaColor>();

		// Token: 0x04000080 RID: 128
		public static readonly IReadOnlyList<ManaColor> WUBRGC = new ManaColor[]
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green,
			ManaColor.Colorless
		}.AsReadOnly<ManaColor>();

		// Token: 0x04000081 RID: 129
		public static readonly IReadOnlyList<ManaColor> WUBRGCP = new ManaColor[]
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green,
			ManaColor.Colorless,
			ManaColor.Philosophy
		}.AsReadOnly<ManaColor>();

		// Token: 0x04000082 RID: 130
		public static readonly IReadOnlyList<ManaColor> AWUBRGCP = new ManaColor[]
		{
			ManaColor.Any,
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green,
			ManaColor.Colorless,
			ManaColor.Philosophy
		}.AsReadOnly<ManaColor>();

		// Token: 0x04000083 RID: 131
		public static readonly IReadOnlyList<ManaColor> WUBRGCH = new ManaColor[]
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green,
			ManaColor.Colorless,
			ManaColor.Hybrid
		}.AsReadOnly<ManaColor>();

		// Token: 0x04000084 RID: 132
		public static readonly IReadOnlyList<ManaColor> AWUBRGCPH = new ManaColor[]
		{
			ManaColor.Any,
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green,
			ManaColor.Colorless,
			ManaColor.Philosophy,
			ManaColor.Hybrid
		}.AsReadOnly<ManaColor>();

		// Token: 0x04000085 RID: 133
		public static readonly IReadOnlyList<ManaColor> Colors = ManaColors.WUBRGCP;

		// Token: 0x04000086 RID: 134
		public static readonly IReadOnlyList<ManaColor> ColorsTrivialReversed = new ManaColor[]
		{
			ManaColor.Green,
			ManaColor.Red,
			ManaColor.Black,
			ManaColor.Blue,
			ManaColor.White,
			ManaColor.Colorless,
			ManaColor.Philosophy
		}.AsReadOnly<ManaColor>();

		// Token: 0x04000087 RID: 135
		public static readonly IReadOnlyList<ManaColor> TrivialColors = ManaColors.WUBRG;

		// Token: 0x04000088 RID: 136
		public static readonly IReadOnlyList<ManaColor> SingleColors = ManaColors.WUBRGC;

		// Token: 0x04000089 RID: 137
		public static readonly IReadOnlyList<ManaColor> SingleColorsWithHybrid = ManaColors.WUBRGCH;

		// Token: 0x0400008A RID: 138
		public static readonly IReadOnlyList<ManaColor> TrivialColorsWithPhilosophy = ManaColors.WUBRGP;

		// Token: 0x0400008B RID: 139
		public static readonly IReadOnlyList<ManaColor> ColorsWithAny = ManaColors.AWUBRGCP;

		// Token: 0x0400008C RID: 140
		public static readonly IReadOnlyList<ManaColor> AllColorsWithHybrid = ManaColors.AWUBRGCPH;
	}
}
