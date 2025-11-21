using System;
using System.IO;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.Base
{
	// Token: 0x02000012 RID: 18
	public sealed class ManaColorConverter : IConfigValueConverter<ManaColor>
	{
		// Token: 0x0600002E RID: 46 RVA: 0x00002A04 File Offset: 0x00000C04
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

		// Token: 0x0600002F RID: 47 RVA: 0x00002A9B File Offset: 0x00000C9B
		public void WriteTo(BinaryWriter writer, ManaColor value)
		{
			writer.Write((int)value);
		}

		// Token: 0x06000030 RID: 48 RVA: 0x00002AA4 File Offset: 0x00000CA4
		public ManaColor ReadFrom(BinaryReader reader)
		{
			return (ManaColor)reader.ReadInt32();
		}

		// Token: 0x06000031 RID: 49 RVA: 0x00002AAC File Offset: 0x00000CAC
		public string ToString(ManaColor value)
		{
			return value.ToShortName().ToString();
		}
	}
}
