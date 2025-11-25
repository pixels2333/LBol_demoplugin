using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
namespace LBoL.Base
{
	public static class ManaColors
	{
		private static ManaColor? TryGetByIndex(int index)
		{
			if (index >= 0 && index < ManaColors.ColorTable.Length)
			{
				return ManaColors.ColorTable[index];
			}
			return default(ManaColor?);
		}
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
		public static ManaColor Parse(string value)
		{
			ManaColor? manaColor = ManaColors.TryGetByIndex(ManaColors.Names.IndexOf(value));
			if (manaColor == null)
			{
				throw new ArgumentException(value + " is not a valid ManaColor");
			}
			return manaColor.GetValueOrDefault();
		}
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
		public static char GetShortName(ManaColor color)
		{
			char? c = ManaColors.ShortNames[(int)color];
			if (c == null)
			{
				throw new ArgumentException(string.Format("Invalid {0}: {1}", "ManaColor", color));
			}
			return c.GetValueOrDefault();
		}
		public static string GetLongName(ManaColor color)
		{
			string text = ManaColors.Names[(int)color];
			if (text == null)
			{
				throw new ArgumentException(string.Format("Invalid {0}: {1}", "ManaColor", color));
			}
			return text;
		}
		public static ManaColor? FromShortName(char name)
		{
			int num = ManaColors.ShortNames.IndexOf(new char?(char.ToUpperInvariant(name)));
			if (num < 0)
			{
				return default(ManaColor?);
			}
			return ManaColors.ColorTable[num];
		}
		private static readonly string[] Names = new string[] { "Any", "White", "Blue", "Black", "Red", "Green", "Colorless", "Philosophy", "Hybrid" };
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
		public static readonly IReadOnlyList<ManaColor> WUBRG = new ManaColor[]
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green
		}.AsReadOnly<ManaColor>();
		public static readonly IReadOnlyList<ManaColor> WUBRGP = new ManaColor[]
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green,
			ManaColor.Philosophy
		}.AsReadOnly<ManaColor>();
		public static readonly IReadOnlyList<ManaColor> WUBRGC = new ManaColor[]
		{
			ManaColor.White,
			ManaColor.Blue,
			ManaColor.Black,
			ManaColor.Red,
			ManaColor.Green,
			ManaColor.Colorless
		}.AsReadOnly<ManaColor>();
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
		public static readonly IReadOnlyList<ManaColor> Colors = ManaColors.WUBRGCP;
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
		public static readonly IReadOnlyList<ManaColor> TrivialColors = ManaColors.WUBRG;
		public static readonly IReadOnlyList<ManaColor> SingleColors = ManaColors.WUBRGC;
		public static readonly IReadOnlyList<ManaColor> SingleColorsWithHybrid = ManaColors.WUBRGCH;
		public static readonly IReadOnlyList<ManaColor> TrivialColorsWithPhilosophy = ManaColors.WUBRGP;
		public static readonly IReadOnlyList<ManaColor> ColorsWithAny = ManaColors.AWUBRGCP;
		public static readonly IReadOnlyList<ManaColor> AllColorsWithHybrid = ManaColors.AWUBRGCPH;
	}
}
