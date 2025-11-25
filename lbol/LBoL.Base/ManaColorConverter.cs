using System;
using System.IO;
using Untitled.ConfigDataBuilder.Base;
namespace LBoL.Base
{
	public sealed class ManaColorConverter : IConfigValueConverter<ManaColor>
	{
		public ManaColor Parse(string value)
		{
			if (value != null)
			{
				string text = value.Trim();
				ManaColor manaColor;
				if (ManaColors.TryParse(text, out manaColor))
				{
					return manaColor;
				}
				if (text.Length == 1)
				{
					ManaColor? manaColor2 = ManaColors.FromShortName(text.get_Chars(0));
					if (manaColor2 != null)
					{
						return manaColor2.GetValueOrDefault();
					}
				}
			}
			throw new InvalidCastException(string.Concat(new string[]
			{
				"Cannot convert '",
				value,
				"'(",
				((value != null) ? value.GetType().Name : null) ?? "null",
				") to ManaColor"
			}));
		}
		public void WriteTo(BinaryWriter writer, ManaColor value)
		{
			writer.Write((int)value);
		}
		public ManaColor ReadFrom(BinaryReader reader)
		{
			return (ManaColor)reader.ReadInt32();
		}
		public string ToString(ManaColor value)
		{
			return value.ToShortName().ToString();
		}
	}
}
