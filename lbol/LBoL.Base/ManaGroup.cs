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
	[ConfigValueConverter(typeof(ManaGroupConverter), new string[] { })]
	[StructLayout(2)]
	public struct ManaGroup : IEquatable<ManaGroup>
	{
		public int Any { readonly get; set; }
		public int White { readonly get; set; }
		public int Blue { readonly get; set; }
		public int Black { readonly get; set; }
		public int Red { readonly get; set; }
		public int Green { readonly get; set; }
		public int Colorless { readonly get; set; }
		public int Philosophy { readonly get; set; }
		public int Hybrid { readonly get; set; }
		public int HybridColor { readonly get; set; }
		public bool IsEmpty
		{
			get
			{
				return this.Any == 0 && this.White == 0 && this.Blue == 0 && this.Black == 0 && this.Red == 0 && this.Green == 0 && this.Colorless == 0 && this.Philosophy == 0 && this.Hybrid == 0;
			}
		}
		public unsafe int GetValue(ManaColor color)
		{
			if (color < ManaColor.Any || color >= (ManaColor)9)
			{
				throw new ArgumentException(string.Format("Invalid {0} for {1} (value = {2})", "ManaColor", base.GetType().Name, color));
			}
			return *((ref this._buffer.FixedElementField) + (IntPtr)color * 4);
		}
		public unsafe void SetValue(ManaColor color, int value)
		{
			if (color < ManaColor.Any || color >= (ManaColor)9)
			{
				throw new ArgumentException(string.Format("Invalid {0} for {1} (value = {2})", "ManaColor", base.GetType().Name, color));
			}
			*((ref this._buffer.FixedElementField) + (IntPtr)color * 4) = value;
		}
		public bool HasColor(ManaColor color)
		{
			return this.GetValue(color) > 0;
		}
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
		public static ManaGroup Single(ManaColor color)
		{
			ManaGroup manaGroup = default(ManaGroup);
			manaGroup[color] = 1;
			return manaGroup;
		}
		public static ManaGroup FromColor(ManaColor color, int amount)
		{
			ManaGroup manaGroup = default(ManaGroup);
			manaGroup[color] = amount;
			return manaGroup;
		}
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
		public static ManaGroup Anys(int value)
		{
			return new ManaGroup
			{
				Any = value
			};
		}
		public static ManaGroup Whites(int value)
		{
			return new ManaGroup
			{
				White = value
			};
		}
		public static ManaGroup Blues(int value)
		{
			return new ManaGroup
			{
				Blue = value
			};
		}
		public static ManaGroup Blacks(int value)
		{
			return new ManaGroup
			{
				Black = value
			};
		}
		public static ManaGroup Reds(int value)
		{
			return new ManaGroup
			{
				Red = value
			};
		}
		public static ManaGroup Greens(int value)
		{
			return new ManaGroup
			{
				Green = value
			};
		}
		public static ManaGroup Colorlesses(int value)
		{
			return new ManaGroup
			{
				Colorless = value
			};
		}
		public static ManaGroup Philosophies(int value)
		{
			return new ManaGroup
			{
				Philosophy = value
			};
		}
		public static ManaGroup Hybrids(int value, int color)
		{
			ManaGroup manaGroup = new ManaGroup
			{
				Hybrid = value
			};
			manaGroup.SetHybridColor(color);
			return manaGroup;
		}
		public static ManaGroup Hybrids(int value, ManaColor color1, ManaColor color2)
		{
			ManaGroup manaGroup = new ManaGroup
			{
				Hybrid = value
			};
			manaGroup.SetHybridColor(color1, color2);
			return manaGroup;
		}
		public static ManaGroup Parse(string shortString)
		{
			ManaGroup manaGroup;
			if (!ManaGroup.TryParse(shortString, out manaGroup))
			{
				throw new ArgumentException("Cannot convert " + shortString + " to ManaGroup");
			}
			return manaGroup;
		}
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
		public bool CanAffordExact(ManaGroup cost)
		{
			if (this.Any != 0)
			{
				throw new InvalidOperationException(string.Format("{0}.{1} invoked with any = {2}, which mustn't be available mana.", "ManaGroup", "CanAffordExact", this.Any));
			}
			return (this - cost).IsValid;
		}
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
		public ManaGroup Intersect(ManaGroup other)
		{
			return ManaGroup.Intersect(this, other);
		}
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
		public ManaGroup Union(ManaGroup other)
		{
			return ManaGroup.Union(this, other);
		}
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
		public int Amount
		{
			get
			{
				return this.Any + this.White + this.Blue + this.Black + this.Red + this.Green + this.Colorless + this.Philosophy + this.Hybrid;
			}
		}
		public int Total
		{
			get
			{
				return this.Amount;
			}
		}
		public ManaColor MaxColor
		{
			get
			{
				return ManaColors.Colors.MaxBy(new Func<ManaColor, int>(this.GetValue));
			}
		}
		public ManaColor MaxTrivialColor
		{
			get
			{
				return ManaColors.TrivialColors.MaxBy(new Func<ManaColor, int>(this.GetValue));
			}
		}
		public int MaxTrivialColorAmount
		{
			get
			{
				return this.GetValue(this.MaxTrivialColor);
			}
		}
		public bool HasTrivial
		{
			get
			{
				return this.White > 0 || this.Blue > 0 || this.Black > 0 || this.Red > 0 || this.Green > 0;
			}
		}
		public bool HasTrivialOrHybrid
		{
			get
			{
				return this.HasTrivial || this.Hybrid > 0;
			}
		}
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
		public bool IsValid
		{
			get
			{
				return this.Any >= 0 && this.White >= 0 && this.Blue >= 0 && this.Black >= 0 && this.Red >= 0 && this.Green >= 0 && this.Colorless >= 0 && this.Philosophy >= 0 && this.Hybrid >= 0;
			}
		}
		public bool IsInvalid
		{
			get
			{
				return !this.IsValid;
			}
		}
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
		public IEnumerable<ManaColor> EnumerateColors()
		{
			return Enumerable.Where<ManaColor>(ManaColors.Colors, new Func<ManaColor, bool>(this.HasColor));
		}
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
		private static bool Equals(ManaGroup lhs, ManaGroup rhs)
		{
			return ((lhs.Hybrid == 0 && rhs.Hybrid == 0) || lhs.HybridColor == rhs.HybridColor) && Enumerable.All<ManaColor>(ManaColors.AllColorsWithHybrid, (ManaColor color) => lhs[color] == rhs[color]);
		}
		public bool Equals(ManaGroup other)
		{
			return ManaGroup.Equals(this, other);
		}
		public override bool Equals([MaybeNull] object obj)
		{
			if (obj is ManaGroup)
			{
				ManaGroup manaGroup = (ManaGroup)obj;
				return ManaGroup.Equals(this, manaGroup);
			}
			return false;
		}
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
		public static bool operator ==(ManaGroup lhs, ManaGroup rhs)
		{
			return ManaGroup.Equals(lhs, rhs);
		}
		public static bool operator !=(ManaGroup lhs, ManaGroup rhs)
		{
			return !ManaGroup.Equals(lhs, rhs);
		}
		public List<ManaColor> GetHybridColors
		{
			get
			{
				return ManaGroup.HybridColors[this.HybridColor];
			}
		}
		public void SetHybridColor(int hybridColorCode)
		{
			if (hybridColorCode >= 0 && hybridColorCode <= 9)
			{
				this.HybridColor = hybridColorCode;
				return;
			}
			Debug.LogError("Setting Hybrid color: hybridColorCode is out of range.");
		}
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
		public bool AllColorsAre(Predicate<int> predicator)
		{
			return predicator.Invoke(this.Any) && predicator.Invoke(this.White) && predicator.Invoke(this.Blue) && predicator.Invoke(this.Black) && predicator.Invoke(this.Red) && predicator.Invoke(this.Green) && predicator.Invoke(this.Colorless) && predicator.Invoke(this.Philosophy);
		}
		public bool AnyColorIs(Predicate<int> predicator)
		{
			return predicator.Invoke(this.Any) || predicator.Invoke(this.White) || predicator.Invoke(this.Blue) || predicator.Invoke(this.Black) || predicator.Invoke(this.Red) || predicator.Invoke(this.Green) || predicator.Invoke(this.Colorless) || predicator.Invoke(this.Philosophy);
		}
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
		[FixedBuffer(typeof(int), 10)]
		[FieldOffset(0)]
		private ManaGroup.<_buffer>e__FixedBuffer _buffer;
		public static readonly ManaGroup Empty;
		public static readonly List<string> HybridColorShortNames;
		public static readonly List<string> HybridColorShortNamesLoopOrder;
		public static readonly List<List<ManaColor>> HybridColors;
		[CompilerGenerated]
		[UnsafeValueType]
		[StructLayout(0, Size = 40)]
		public struct <_buffer>e__FixedBuffer
		{
			public int FixedElementField;
		}
	}
}
