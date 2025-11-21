using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LBoL.Base.Extensions;
using UnityEngine;
using Untitled.ConfigDataBuilder.Base;

namespace LBoL.Base
{
	// Token: 0x02000013 RID: 19
	[ConfigValueConverter(typeof(ManaGroupConverter), new string[] { })]
	[StructLayout(2)]
	public struct ManaGroup : IEquatable<ManaGroup>
	{
		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00002ACF File Offset: 0x00000CCF
		// (set) Token: 0x06000034 RID: 52 RVA: 0x00002AD7 File Offset: 0x00000CD7
		public int Any { readonly get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000035 RID: 53 RVA: 0x00002AE0 File Offset: 0x00000CE0
		// (set) Token: 0x06000036 RID: 54 RVA: 0x00002AE8 File Offset: 0x00000CE8
		public int White { readonly get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00002AF1 File Offset: 0x00000CF1
		// (set) Token: 0x06000038 RID: 56 RVA: 0x00002AF9 File Offset: 0x00000CF9
		public int Blue { readonly get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00002B02 File Offset: 0x00000D02
		// (set) Token: 0x0600003A RID: 58 RVA: 0x00002B0A File Offset: 0x00000D0A
		public int Black { readonly get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00002B13 File Offset: 0x00000D13
		// (set) Token: 0x0600003C RID: 60 RVA: 0x00002B1B File Offset: 0x00000D1B
		public int Red { readonly get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002B24 File Offset: 0x00000D24
		// (set) Token: 0x0600003E RID: 62 RVA: 0x00002B2C File Offset: 0x00000D2C
		public int Green { readonly get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600003F RID: 63 RVA: 0x00002B35 File Offset: 0x00000D35
		// (set) Token: 0x06000040 RID: 64 RVA: 0x00002B3D File Offset: 0x00000D3D
		public int Colorless { readonly get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000041 RID: 65 RVA: 0x00002B46 File Offset: 0x00000D46
		// (set) Token: 0x06000042 RID: 66 RVA: 0x00002B4E File Offset: 0x00000D4E
		public int Philosophy { readonly get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000043 RID: 67 RVA: 0x00002B57 File Offset: 0x00000D57
		// (set) Token: 0x06000044 RID: 68 RVA: 0x00002B5F File Offset: 0x00000D5F
		public int Hybrid { readonly get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000045 RID: 69 RVA: 0x00002B68 File Offset: 0x00000D68
		// (set) Token: 0x06000046 RID: 70 RVA: 0x00002B70 File Offset: 0x00000D70
		public int HybridColor { readonly get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000047 RID: 71 RVA: 0x00002B7C File Offset: 0x00000D7C
		public bool IsEmpty
		{
			get
			{
				return this.Any == 0 && this.White == 0 && this.Blue == 0 && this.Black == 0 && this.Red == 0 && this.Green == 0 && this.Colorless == 0 && this.Philosophy == 0 && this.Hybrid == 0;
			}
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00002BD4 File Offset: 0x00000DD4
		public unsafe int GetValue(ManaColor color)
		{
			if (color < ManaColor.Any || color >= (ManaColor)9)
			{
				throw new ArgumentException(string.Format("Invalid {0} for {1} (value = {2})", "ManaColor", base.GetType().Name, color));
			}
			return *((ref this._buffer.FixedElementField) + (IntPtr)color * 4);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00002C30 File Offset: 0x00000E30
		public unsafe void SetValue(ManaColor color, int value)
		{
			if (color < ManaColor.Any || color >= (ManaColor)9)
			{
				throw new ArgumentException(string.Format("Invalid {0} for {1} (value = {2})", "ManaColor", base.GetType().Name, color));
			}
			*((ref this._buffer.FixedElementField) + (IntPtr)color * 4) = value;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00002C8A File Offset: 0x00000E8A
		public bool HasColor(ManaColor color)
		{
			return this.GetValue(color) > 0;
		}

		// Token: 0x17000015 RID: 21
		public int this[ManaColor color]
		{
			get
			{
				return this.GetValue(color);
			}
			set
			{
				this.SetValue(color, value);
			}
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002CAC File Offset: 0x00000EAC
		public void WriteTo(BinaryWriter writer)
		{
			writer.Write(this.Any);
			writer.Write(this.White);
			writer.Write(this.Blue);
			writer.Write(this.Black);
			writer.Write(this.Red);
			writer.Write(this.Green);
			writer.Write(this.Colorless);
			writer.Write(this.Philosophy);
			writer.Write(this.Hybrid);
			writer.Write(this.HybridColor);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00002D34 File Offset: 0x00000F34
		public static ManaGroup FromBinary(BinaryReader reader)
		{
			return new ManaGroup
			{
				Any = reader.ReadInt32(),
				White = reader.ReadInt32(),
				Blue = reader.ReadInt32(),
				Black = reader.ReadInt32(),
				Red = reader.ReadInt32(),
				Green = reader.ReadInt32(),
				Colorless = reader.ReadInt32(),
				Philosophy = reader.ReadInt32(),
				Hybrid = reader.ReadInt32(),
				HybridColor = reader.ReadInt32()
			};
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00002DCC File Offset: 0x00000FCC
		public static ManaGroup Single(ManaColor color)
		{
			ManaGroup manaGroup = default(ManaGroup);
			manaGroup[color] = 1;
			return manaGroup;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00002DF0 File Offset: 0x00000FF0
		public static ManaGroup FromColor(ManaColor color, int amount)
		{
			ManaGroup manaGroup = default(ManaGroup);
			manaGroup[color] = amount;
			return manaGroup;
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00002E14 File Offset: 0x00001014
		public static ManaGroup FromComponents(IEnumerable<ManaColor> components)
		{
			ManaGroup empty = ManaGroup.Empty;
			foreach (ManaColor manaColor in components)
			{
				ManaColor manaColor2 = manaColor;
				int num = empty[manaColor2] + 1;
				empty[manaColor2] = num;
			}
			return empty;
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00002E74 File Offset: 0x00001074
		public static ManaGroup Anys(int value)
		{
			return new ManaGroup
			{
				Any = value
			};
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00002E94 File Offset: 0x00001094
		public static ManaGroup Whites(int value)
		{
			return new ManaGroup
			{
				White = value
			};
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00002EB4 File Offset: 0x000010B4
		public static ManaGroup Blues(int value)
		{
			return new ManaGroup
			{
				Blue = value
			};
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00002ED4 File Offset: 0x000010D4
		public static ManaGroup Blacks(int value)
		{
			return new ManaGroup
			{
				Black = value
			};
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00002EF4 File Offset: 0x000010F4
		public static ManaGroup Reds(int value)
		{
			return new ManaGroup
			{
				Red = value
			};
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00002F14 File Offset: 0x00001114
		public static ManaGroup Greens(int value)
		{
			return new ManaGroup
			{
				Green = value
			};
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00002F34 File Offset: 0x00001134
		public static ManaGroup Colorlesses(int value)
		{
			return new ManaGroup
			{
				Colorless = value
			};
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00002F54 File Offset: 0x00001154
		public static ManaGroup Philosophies(int value)
		{
			return new ManaGroup
			{
				Philosophy = value
			};
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00002F74 File Offset: 0x00001174
		public static ManaGroup Hybrids(int value, int color)
		{
			ManaGroup manaGroup = new ManaGroup
			{
				Hybrid = value
			};
			manaGroup.SetHybridColor(color);
			return manaGroup;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00002F9C File Offset: 0x0000119C
		public static ManaGroup Hybrids(int value, ManaColor color1, ManaColor color2)
		{
			ManaGroup manaGroup = new ManaGroup
			{
				Hybrid = value
			};
			manaGroup.SetHybridColor(color1, color2);
			return manaGroup;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00002FC8 File Offset: 0x000011C8
		public static ManaGroup Parse(string shortString)
		{
			ManaGroup manaGroup;
			if (!ManaGroup.TryParse(shortString, out manaGroup))
			{
				throw new ArgumentException("Cannot convert " + shortString + " to ManaGroup");
			}
			return manaGroup;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00002FF8 File Offset: 0x000011F8
		public static bool TryParse(string shortString, out ManaGroup result)
		{
			result = default(ManaGroup);
			int i = 0;
			int num = 0;
			while (i < shortString.Length && char.IsDigit(shortString.get_Chars(i)))
			{
				num = num * 10 + (int)(shortString.get_Chars(i) - '0');
				i++;
			}
			result.Any += num;
			while (i < shortString.Length)
			{
				char c = shortString.get_Chars(i);
				if (c == ':')
				{
					if (i != shortString.Length - 3)
					{
						return false;
					}
					ManaColor? manaColor = ManaColors.FromShortName(shortString.get_Chars(i + 1));
					ManaColor? manaColor2 = ManaColors.FromShortName(shortString.get_Chars(i + 2));
					if (manaColor == null || manaColor2 == null)
					{
						return false;
					}
					result.SetHybridColor(manaColor.Value, manaColor2.Value);
					return true;
				}
				else
				{
					if (c == '{')
					{
						i++;
						num = 0;
						while (i < shortString.Length)
						{
							char c2 = shortString.get_Chars(i);
							if (c2 < '0' || c2 > '9')
							{
								break;
							}
							num = num * 10 + (int)(c2 - '0');
							i++;
						}
						if (i >= shortString.Length)
						{
							return false;
						}
						ManaColor? manaColor3 = ManaColors.FromShortName(shortString.get_Chars(i));
						if (manaColor3 == null)
						{
							return false;
						}
						i++;
						if (i >= shortString.Length || shortString.get_Chars(i) != '}')
						{
							return false;
						}
						ref ManaGroup ptr = ref result;
						ManaColor manaColor4 = manaColor3.Value;
						ptr[manaColor4] += num;
					}
					else
					{
						ManaColor? manaColor5 = ManaColors.FromShortName(shortString.get_Chars(i));
						if (manaColor5 == null)
						{
							return false;
						}
						ManaColor manaColor4 = manaColor5.Value;
						int num2 = result[manaColor4] + 1;
						result[manaColor4] = num2;
					}
					i++;
				}
			}
			return true;
		}

		// Token: 0x0600005E RID: 94 RVA: 0x0000319C File Offset: 0x0000139C
		public static ManaGroup operator +(ManaGroup lhs, ManaGroup rhs)
		{
			return new ManaGroup
			{
				Any = lhs.Any + rhs.Any,
				White = lhs.White + rhs.White,
				Blue = lhs.Blue + rhs.Blue,
				Black = lhs.Black + rhs.Black,
				Red = lhs.Red + rhs.Red,
				Green = lhs.Green + rhs.Green,
				Colorless = lhs.Colorless + rhs.Colorless,
				Philosophy = lhs.Philosophy + rhs.Philosophy,
				Hybrid = lhs.Hybrid + rhs.Hybrid,
				HybridColor = ((lhs.Hybrid != 0) ? lhs.HybridColor : ((rhs.Hybrid != 0) ? rhs.HybridColor : lhs.HybridColor))
			};
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000032AC File Offset: 0x000014AC
		public static ManaGroup operator -(ManaGroup lhs, ManaGroup rhs)
		{
			return new ManaGroup
			{
				Any = lhs.Any - rhs.Any,
				White = lhs.White - rhs.White,
				Blue = lhs.Blue - rhs.Blue,
				Black = lhs.Black - rhs.Black,
				Red = lhs.Red - rhs.Red,
				Green = lhs.Green - rhs.Green,
				Colorless = lhs.Colorless - rhs.Colorless,
				Philosophy = lhs.Philosophy - rhs.Philosophy,
				Hybrid = lhs.Hybrid - rhs.Hybrid,
				HybridColor = ((lhs.Hybrid != 0) ? lhs.HybridColor : ((rhs.Hybrid != 0) ? rhs.HybridColor : lhs.HybridColor))
			};
		}

		// Token: 0x06000060 RID: 96 RVA: 0x000033BC File Offset: 0x000015BC
		public static ManaGroup operator *(ManaGroup mana, int multiplier)
		{
			return new ManaGroup
			{
				Any = mana.Any * multiplier,
				White = mana.White * multiplier,
				Blue = mana.Blue * multiplier,
				Black = mana.Black * multiplier,
				Red = mana.Red * multiplier,
				Green = mana.Green * multiplier,
				Colorless = mana.Colorless * multiplier,
				Philosophy = mana.Philosophy * multiplier,
				Hybrid = mana.Hybrid * multiplier
			};
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00003464 File Offset: 0x00001664
		public bool CanAffordExact(ManaGroup cost)
		{
			if (this.Any != 0)
			{
				throw new InvalidOperationException(string.Format("{0}.{1} invoked with any = {2}, which mustn't be available mana.", "ManaGroup", "CanAffordExact", this.Any));
			}
			return (this - cost).IsValid;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x000034B4 File Offset: 0x000016B4
		public bool CanAfford(ManaGroup cost)
		{
			if (this.Any != 0)
			{
				throw new InvalidOperationException(string.Format("{0}.{1} invoked with any = {2}, which mustn't be available mana.", "ManaGroup", "CanAfford", this.Any));
			}
			if (this.Hybrid != 0)
			{
				throw new InvalidOperationException(string.Format("{0}.{1} invoked with hybrid = {2}, which mustn't be available mana.", "ManaGroup", "CanAfford", this.Hybrid));
			}
			ManaGroup manaGroup = this - cost;
			manaGroup.Any = 0;
			if (manaGroup.Amount < cost.Any)
			{
				return false;
			}
			int num = manaGroup.Philosophy;
			foreach (ManaColor manaColor in ManaColors.SingleColors)
			{
				int num2 = manaGroup[manaColor];
				if (num2 < 0)
				{
					num += num2;
				}
			}
			if (cost.Hybrid > 0)
			{
				List<ManaColor> list = ManaGroup.HybridColors[cost.HybridColor];
				ManaColor manaColor2 = list[0];
				ManaColor manaColor3 = list[1];
				int num3 = Math.Max(0, manaGroup[manaColor2]);
				int num4 = Math.Max(0, manaGroup[manaColor3]);
				int num5 = num3 + num4 - cost.Hybrid;
				if (num5 < 0)
				{
					num += num5;
				}
			}
			return num >= 0;
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00003600 File Offset: 0x00001800
		public static ManaGroup Intersect(ManaGroup a, ManaGroup b)
		{
			return new ManaGroup
			{
				Any = Math.Min(a.Any, b.Any),
				White = Math.Min(a.White, b.White),
				Blue = Math.Min(a.Blue, b.Blue),
				Black = Math.Min(a.Black, b.Black),
				Red = Math.Min(a.Red, b.Red),
				Green = Math.Min(a.Green, b.Green),
				Colorless = Math.Min(a.Colorless, b.Colorless),
				Philosophy = Math.Min(a.Philosophy, b.Philosophy),
				Hybrid = Math.Min(a.Hybrid, b.Hybrid),
				HybridColor = ((a.Hybrid != 0) ? a.HybridColor : ((b.Hybrid != 0) ? b.HybridColor : a.HybridColor))
			};
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00003732 File Offset: 0x00001932
		public ManaGroup Intersect(ManaGroup other)
		{
			return ManaGroup.Intersect(this, other);
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00003740 File Offset: 0x00001940
		public static ManaGroup Union(ManaGroup a, ManaGroup b)
		{
			return new ManaGroup
			{
				Any = Math.Max(a.Any, b.Any),
				White = Math.Max(a.White, b.White),
				Blue = Math.Max(a.Blue, b.Blue),
				Black = Math.Max(a.Black, b.Black),
				Red = Math.Max(a.Red, b.Red),
				Green = Math.Max(a.Green, b.Green),
				Colorless = Math.Max(a.Colorless, b.Colorless),
				Philosophy = Math.Max(a.Philosophy, b.Philosophy),
				Hybrid = Math.Max(a.Hybrid, b.Hybrid),
				HybridColor = ((a.Hybrid != 0) ? a.HybridColor : ((b.Hybrid != 0) ? b.HybridColor : a.HybridColor))
			};
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00003872 File Offset: 0x00001A72
		public ManaGroup Union(ManaGroup other)
		{
			return ManaGroup.Union(this, other);
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00003880 File Offset: 0x00001A80
		public ManaGroup ClampComponentMax(int componentMax)
		{
			return new ManaGroup
			{
				Any = Math.Min(this.Any, componentMax),
				White = Math.Min(this.White, componentMax),
				Blue = Math.Min(this.Blue, componentMax),
				Black = Math.Min(this.Black, componentMax),
				Red = Math.Min(this.Red, componentMax),
				Green = Math.Min(this.Green, componentMax),
				Colorless = Math.Min(this.Colorless, componentMax),
				Philosophy = Math.Min(this.Philosophy, componentMax),
				Hybrid = Math.Min(this.Hybrid, componentMax),
				HybridColor = this.HybridColor
			};
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x06000068 RID: 104 RVA: 0x0000394E File Offset: 0x00001B4E
		public int Amount
		{
			get
			{
				return this.Any + this.White + this.Blue + this.Black + this.Red + this.Green + this.Colorless + this.Philosophy + this.Hybrid;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000069 RID: 105 RVA: 0x0000398E File Offset: 0x00001B8E
		public int Total
		{
			get
			{
				return this.Amount;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x0600006A RID: 106 RVA: 0x00003996 File Offset: 0x00001B96
		public ManaColor MaxColor
		{
			get
			{
				return ManaColors.Colors.MaxBy(new Func<ManaColor, int>(this.GetValue));
			}
		}

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600006B RID: 107 RVA: 0x000039B8 File Offset: 0x00001BB8
		public ManaColor MaxTrivialColor
		{
			get
			{
				return ManaColors.TrivialColors.MaxBy(new Func<ManaColor, int>(this.GetValue));
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600006C RID: 108 RVA: 0x000039DA File Offset: 0x00001BDA
		public int MaxTrivialColorAmount
		{
			get
			{
				return this.GetValue(this.MaxTrivialColor);
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600006D RID: 109 RVA: 0x000039E8 File Offset: 0x00001BE8
		public bool HasTrivial
		{
			get
			{
				return this.White > 0 || this.Blue > 0 || this.Black > 0 || this.Red > 0 || this.Green > 0;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600006E RID: 110 RVA: 0x00003A19 File Offset: 0x00001C19
		public bool HasTrivialOrHybrid
		{
			get
			{
				return this.HasTrivial || this.Hybrid > 0;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00003A30 File Offset: 0x00001C30
		public int ColorCount
		{
			get
			{
				int num = 0;
				if (this.Any > 0)
				{
					num++;
				}
				if (this.White > 0)
				{
					num++;
				}
				if (this.Blue > 0)
				{
					num++;
				}
				if (this.Black > 0)
				{
					num++;
				}
				if (this.Red > 0)
				{
					num++;
				}
				if (this.Green > 0)
				{
					num++;
				}
				if (this.Colorless > 0)
				{
					num++;
				}
				if (this.Philosophy > 0)
				{
					num++;
				}
				return num;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000070 RID: 112 RVA: 0x00003AA8 File Offset: 0x00001CA8
		public int SingleColorCount
		{
			get
			{
				int num = 0;
				if (this.White > 0)
				{
					num++;
				}
				if (this.Blue > 0)
				{
					num++;
				}
				if (this.Black > 0)
				{
					num++;
				}
				if (this.Red > 0)
				{
					num++;
				}
				if (this.Green > 0)
				{
					num++;
				}
				if (this.Colorless > 0)
				{
					num++;
				}
				return num;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x06000071 RID: 113 RVA: 0x00003B08 File Offset: 0x00001D08
		public int TrivialColorCount
		{
			get
			{
				int num = 0;
				if (this.White > 0)
				{
					num++;
				}
				if (this.Red > 0)
				{
					num++;
				}
				if (this.Green > 0)
				{
					num++;
				}
				if (this.Black > 0)
				{
					num++;
				}
				if (this.Blue > 0)
				{
					num++;
				}
				return num;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000072 RID: 114 RVA: 0x00003B5C File Offset: 0x00001D5C
		public bool IsValid
		{
			get
			{
				return this.Any >= 0 && this.White >= 0 && this.Blue >= 0 && this.Black >= 0 && this.Red >= 0 && this.Green >= 0 && this.Colorless >= 0 && this.Philosophy >= 0 && this.Hybrid >= 0;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000073 RID: 115 RVA: 0x00003BBF File Offset: 0x00001DBF
		public bool IsInvalid
		{
			get
			{
				return !this.IsValid;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000074 RID: 116 RVA: 0x00003BCC File Offset: 0x00001DCC
		public ManaGroup Corrected
		{
			get
			{
				ManaGroup manaGroup = this;
				if (this.Any < 0)
				{
					manaGroup.Any = 0;
				}
				if (this.White < 0)
				{
					manaGroup.White = 0;
				}
				if (this.Blue < 0)
				{
					manaGroup.Blue = 0;
				}
				if (this.Black < 0)
				{
					manaGroup.Black = 0;
				}
				if (this.Red < 0)
				{
					manaGroup.Red = 0;
				}
				if (this.Green < 0)
				{
					manaGroup.Green = 0;
				}
				if (this.Colorless < 0)
				{
					manaGroup.Colorless = 0;
				}
				if (this.Philosophy < 0)
				{
					manaGroup.Philosophy = 0;
				}
				if (this.Hybrid < 0)
				{
					manaGroup.Hybrid = 0;
				}
				return manaGroup;
			}
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00003C7A File Offset: 0x00001E7A
		public IEnumerable<ManaColor> EnumerateColors()
		{
			return Enumerable.Where<ManaColor>(ManaColors.Colors, new Func<ManaColor, bool>(this.HasColor));
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00003C9C File Offset: 0x00001E9C
		public IEnumerable<ManaColor> EnumerateComponents()
		{
			int num;
			for (int i = 0; i < this.Any; i = num)
			{
				yield return ManaColor.Any;
				num = i + 1;
			}
			for (int i = 0; i < this.White; i = num)
			{
				yield return ManaColor.White;
				num = i + 1;
			}
			for (int i = 0; i < this.Blue; i = num)
			{
				yield return ManaColor.Blue;
				num = i + 1;
			}
			for (int i = 0; i < this.Black; i = num)
			{
				yield return ManaColor.Black;
				num = i + 1;
			}
			for (int i = 0; i < this.Red; i = num)
			{
				yield return ManaColor.Red;
				num = i + 1;
			}
			for (int i = 0; i < this.Green; i = num)
			{
				yield return ManaColor.Green;
				num = i + 1;
			}
			for (int i = 0; i < this.Colorless; i = num)
			{
				yield return ManaColor.Colorless;
				num = i + 1;
			}
			for (int i = 0; i < this.Philosophy; i = num)
			{
				yield return ManaColor.Philosophy;
				num = i + 1;
			}
			for (int i = 0; i < this.Hybrid; i = num)
			{
				yield return ManaColor.Hybrid;
				num = i + 1;
			}
			yield break;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00003CB4 File Offset: 0x00001EB4
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.Any > 0)
			{
				stringBuilder.Append(this.Any);
			}
			else if (this.Any < 0)
			{
				stringBuilder.Append("(").Append(this.Any).Append(")");
			}
			ManaGroup.<ToString>g__AppendValue|105_0(stringBuilder, ManaColor.White, this.White);
			ManaGroup.<ToString>g__AppendValue|105_0(stringBuilder, ManaColor.Blue, this.Blue);
			ManaGroup.<ToString>g__AppendValue|105_0(stringBuilder, ManaColor.Black, this.Black);
			ManaGroup.<ToString>g__AppendValue|105_0(stringBuilder, ManaColor.Red, this.Red);
			ManaGroup.<ToString>g__AppendValue|105_0(stringBuilder, ManaColor.Green, this.Green);
			ManaGroup.<ToString>g__AppendValue|105_0(stringBuilder, ManaColor.Colorless, this.Colorless);
			ManaGroup.<ToString>g__AppendValue|105_0(stringBuilder, ManaColor.Philosophy, this.Philosophy);
			ManaGroup.<ToString>g__AppendValue|105_0(stringBuilder, ManaColor.Hybrid, this.Hybrid);
			if (this.Hybrid > 0)
			{
				stringBuilder.Append(":").Append(ManaGroup.HybridColorShortNames[this.HybridColor]);
			}
			if (stringBuilder.Length != 0)
			{
				return stringBuilder.ToString();
			}
			return "0";
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00003DB0 File Offset: 0x00001FB0
		private static bool Equals(ManaGroup lhs, ManaGroup rhs)
		{
			return ((lhs.Hybrid == 0 && rhs.Hybrid == 0) || lhs.HybridColor == rhs.HybridColor) && Enumerable.All<ManaColor>(ManaColors.AllColorsWithHybrid, (ManaColor color) => lhs[color] == rhs[color]);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00003E1B File Offset: 0x0000201B
		public bool Equals(ManaGroup other)
		{
			return ManaGroup.Equals(this, other);
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00003E2C File Offset: 0x0000202C
		public override bool Equals([MaybeNull] object obj)
		{
			if (obj is ManaGroup)
			{
				ManaGroup manaGroup = (ManaGroup)obj;
				return ManaGroup.Equals(this, manaGroup);
			}
			return false;
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00003E58 File Offset: 0x00002058
		public override int GetHashCode()
		{
			HashCode hashCode = default(HashCode);
			hashCode.Add<int>(this.Any);
			hashCode.Add<int>(this.White);
			hashCode.Add<int>(this.Blue);
			hashCode.Add<int>(this.Black);
			hashCode.Add<int>(this.Red);
			hashCode.Add<int>(this.Green);
			hashCode.Add<int>(this.Colorless);
			hashCode.Add<int>(this.Philosophy);
			hashCode.Add<int>(this.Hybrid);
			hashCode.Add<int>(this.HybridColor);
			return hashCode.ToHashCode();
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00003EF6 File Offset: 0x000020F6
		public static bool operator ==(ManaGroup lhs, ManaGroup rhs)
		{
			return ManaGroup.Equals(lhs, rhs);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00003EFF File Offset: 0x000020FF
		public static bool operator !=(ManaGroup lhs, ManaGroup rhs)
		{
			return !ManaGroup.Equals(lhs, rhs);
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600007E RID: 126 RVA: 0x00003F0B File Offset: 0x0000210B
		public List<ManaColor> GetHybridColors
		{
			get
			{
				return ManaGroup.HybridColors[this.HybridColor];
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00003F1D File Offset: 0x0000211D
		public void SetHybridColor(int hybridColorCode)
		{
			if (hybridColorCode >= 0 && hybridColorCode <= 9)
			{
				this.HybridColor = hybridColorCode;
				return;
			}
			Debug.LogError("Setting Hybrid color: hybridColorCode is out of range.");
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00003F3C File Offset: 0x0000213C
		public void SetHybridColor(ManaColor color1, ManaColor color2)
		{
			if (color1 != color2 && Enumerable.Contains<ManaColor>(ManaColors.TrivialColors, color1) && Enumerable.Contains<ManaColor>(ManaColors.TrivialColors, color2))
			{
				for (int i = 0; i < 10; i++)
				{
					if (ManaGroup.HybridColors[i].Contains(color1) && ManaGroup.HybridColors[i].Contains(color2))
					{
						this.HybridColor = i;
						return;
					}
				}
				return;
			}
			Debug.LogError(string.Format("Setting Hybrid color with wrong colors: {0} and {1}", color1, color2));
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00003FC0 File Offset: 0x000021C0
		public void Visit(Action<int> visitor)
		{
			visitor.Invoke(this.Any);
			visitor.Invoke(this.White);
			visitor.Invoke(this.Blue);
			visitor.Invoke(this.Black);
			visitor.Invoke(this.Red);
			visitor.Invoke(this.Green);
			visitor.Invoke(this.Colorless);
			visitor.Invoke(this.Philosophy);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00004030 File Offset: 0x00002230
		public void Visit(Func<int, int> visitor)
		{
			this.Any = visitor.Invoke(this.Any);
			this.White = visitor.Invoke(this.White);
			this.Blue = visitor.Invoke(this.Blue);
			this.Black = visitor.Invoke(this.Black);
			this.Red = visitor.Invoke(this.Red);
			this.Green = visitor.Invoke(this.Green);
			this.Colorless = visitor.Invoke(this.Colorless);
			this.Philosophy = visitor.Invoke(this.Philosophy);
		}

		// Token: 0x06000083 RID: 131 RVA: 0x000040D0 File Offset: 0x000022D0
		public bool AllColorsAre(Predicate<int> predicator)
		{
			return predicator.Invoke(this.Any) && predicator.Invoke(this.White) && predicator.Invoke(this.Blue) && predicator.Invoke(this.Black) && predicator.Invoke(this.Red) && predicator.Invoke(this.Green) && predicator.Invoke(this.Colorless) && predicator.Invoke(this.Philosophy);
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00004150 File Offset: 0x00002350
		public bool AnyColorIs(Predicate<int> predicator)
		{
			return predicator.Invoke(this.Any) || predicator.Invoke(this.White) || predicator.Invoke(this.Blue) || predicator.Invoke(this.Black) || predicator.Invoke(this.Red) || predicator.Invoke(this.Green) || predicator.Invoke(this.Colorless) || predicator.Invoke(this.Philosophy);
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000041D0 File Offset: 0x000023D0
		// Note: this type is marked as 'beforefieldinit'.
		static ManaGroup()
		{
			List<string> list = new List<string>();
			list.Add("HWU");
			list.Add("HWB");
			list.Add("HWR");
			list.Add("HWG");
			list.Add("HUB");
			list.Add("HUR");
			list.Add("HUG");
			list.Add("HBR");
			list.Add("HBG");
			list.Add("HRG");
			ManaGroup.HybridColorShortNames = list;
			List<string> list2 = new List<string>();
			list2.Add("HWU");
			list2.Add("HWB");
			list2.Add("HRW");
			list2.Add("HGW");
			list2.Add("HUB");
			list2.Add("HUR");
			list2.Add("HGU");
			list2.Add("HBR");
			list2.Add("HBG");
			list2.Add("HRG");
			ManaGroup.HybridColorShortNamesLoopOrder = list2;
			List<List<ManaColor>> list3 = new List<List<ManaColor>>();
			List<ManaColor> list4 = new List<ManaColor>();
			list4.Add(ManaColor.White);
			list4.Add(ManaColor.Blue);
			list3.Add(list4);
			List<ManaColor> list5 = new List<ManaColor>();
			list5.Add(ManaColor.White);
			list5.Add(ManaColor.Black);
			list3.Add(list5);
			List<ManaColor> list6 = new List<ManaColor>();
			list6.Add(ManaColor.White);
			list6.Add(ManaColor.Red);
			list3.Add(list6);
			List<ManaColor> list7 = new List<ManaColor>();
			list7.Add(ManaColor.White);
			list7.Add(ManaColor.Green);
			list3.Add(list7);
			List<ManaColor> list8 = new List<ManaColor>();
			list8.Add(ManaColor.Blue);
			list8.Add(ManaColor.Black);
			list3.Add(list8);
			List<ManaColor> list9 = new List<ManaColor>();
			list9.Add(ManaColor.Blue);
			list9.Add(ManaColor.Red);
			list3.Add(list9);
			List<ManaColor> list10 = new List<ManaColor>();
			list10.Add(ManaColor.Blue);
			list10.Add(ManaColor.Green);
			list3.Add(list10);
			List<ManaColor> list11 = new List<ManaColor>();
			list11.Add(ManaColor.Black);
			list11.Add(ManaColor.Red);
			list3.Add(list11);
			List<ManaColor> list12 = new List<ManaColor>();
			list12.Add(ManaColor.Black);
			list12.Add(ManaColor.Green);
			list3.Add(list12);
			List<ManaColor> list13 = new List<ManaColor>();
			list13.Add(ManaColor.Red);
			list13.Add(ManaColor.Green);
			list3.Add(list13);
			ManaGroup.HybridColors = list3;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000043D4 File Offset: 0x000025D4
		[CompilerGenerated]
		internal static void <ToString>g__AppendValue|105_0(StringBuilder builder, ManaColor color, int value)
		{
			if (value > 3 || value < 0)
			{
				builder.Append('{').Append(value).Append(color.ToShortName())
					.Append('}');
				return;
			}
			for (int i = 0; i < value; i++)
			{
				builder.Append(color.ToShortName());
			}
		}

		// Token: 0x04000097 RID: 151
		[FixedBuffer(typeof(int), 10)]
		[FieldOffset(0)]
		private ManaGroup.<_buffer>e__FixedBuffer _buffer;

		// Token: 0x04000098 RID: 152
		public static readonly ManaGroup Empty;

		// Token: 0x04000099 RID: 153
		public static readonly List<string> HybridColorShortNames;

		// Token: 0x0400009A RID: 154
		public static readonly List<string> HybridColorShortNamesLoopOrder;

		// Token: 0x0400009B RID: 155
		public static readonly List<List<ManaColor>> HybridColors;

		// Token: 0x0200002F RID: 47
		[CompilerGenerated]
		[UnsafeValueType]
		[StructLayout(0, Size = 40)]
		public struct <_buffer>e__FixedBuffer
		{
			// Token: 0x040000EB RID: 235
			public int FixedElementField;
		}
	}
}
