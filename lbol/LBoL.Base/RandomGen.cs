using System;
namespace LBoL.Base
{
	public sealed class RandomGen
	{
		public static ulong GetRandomSeed()
		{
			return RandomGen.InitGen.NextULong();
		}
		public static bool IsValidSeedChar(char c)
		{
			return (int)c < RandomGen.ReverseTable.Length && RandomGen.ReverseTable[(int)c] != null;
		}
		public static ulong ParseSeed(string seedStr)
		{
			if (seedStr.Length > RandomGen.MaxSeedSize)
			{
				throw new OverflowException();
			}
			ulong num = 0UL;
			int i = 0;
			while (i < seedStr.Length)
			{
				char c = seedStr.get_Chars(i);
				if ((int)c < RandomGen.ReverseTable.Length)
				{
					uint? num2 = RandomGen.ReverseTable[(int)c];
					if (num2 != null)
					{
						uint valueOrDefault = num2.GetValueOrDefault();
						checked
						{
							num = num * unchecked((ulong)RandomGen.Base) + unchecked((ulong)valueOrDefault);
						}
						i++;
						continue;
					}
				}
				throw new ArgumentException(string.Format("Seed strings contains invalid character: {0}", c));
			}
			return num;
		}
		public static bool TryParseSeed(string seedStr, out ulong result)
		{
			bool flag;
			try
			{
				result = RandomGen.ParseSeed(seedStr);
				flag = true;
			}
			catch
			{
				result = 0UL;
				flag = false;
			}
			return flag;
		}
		public unsafe static string SeedToString(ulong seed)
		{
			int maxSeedSize = RandomGen.MaxSeedSize;
			Span<char> span;
			checked
			{
				span = new Span<char>(stackalloc byte[unchecked((UIntPtr)maxSeedSize) * 2], maxSeedSize);
			}
			for (int i = RandomGen.MaxSeedSize - 1; i >= 0; i--)
			{
				ulong num = seed % (ulong)RandomGen.Base;
				seed /= (ulong)RandomGen.Base;
				*span[i] = RandomGen.ForwardTable[(int)(checked((IntPtr)num))];
			}
			return new string(span);
		}
		public ulong State { get; private set; } = 5573589319906701683UL;
		private RandomGen()
		{
		}
		public RandomGen(ulong seed)
		{
			this.State = seed + 1442695040888963407UL;
			this.Next();
		}
		public static RandomGen FromState(ulong state)
		{
			return new RandomGen
			{
				State = state
			};
		}
		public uint Next()
		{
			ulong state = this.State;
			this.State = state * 6364136223846793005UL + 1442695040888963407UL;
			uint num = (uint)(((state >> 18) ^ state) >> 27);
			uint num2 = (uint)(state >> 59);
			return (num >> (int)num2) | (num << (int)((uint)(-(uint)((ulong)num2))));
		}
		public uint Next(uint max)
		{
			if (max == 0U)
			{
				return 0U;
			}
			uint num = max + 1U;
			uint num2 = (uint)(-(uint)((ulong)num)) % num;
			uint num3;
			do
			{
				num3 = this.Next();
			}
			while (num3 < num2);
			return num3 % num;
		}
		public ulong NextULong()
		{
			ulong num = (ulong)this.Next();
			ulong num2 = (ulong)this.Next();
			return (num << 32) | num2;
		}
		public int NextInt(int a, int b)
		{
			if (a > b)
			{
				throw new ArgumentException(string.Format("a({0}) is greater than b{1}", a, b));
			}
			uint num = (uint)(b - a);
			return (int)((ulong)this.Next(num) + (ulong)((long)a));
		}
		public double NextDouble()
		{
			return this.Next() * 1.0 / 4294967296.0;
		}
		public double NextDouble(double a, double b)
		{
			if (a > b)
			{
				throw new ArgumentException(string.Format("a({0}) is greater than b{1}", a, b));
			}
			return this.NextDouble() * (b - a);
		}
		public float NextFloat()
		{
			return this.Next() * 1f / 4.2949673E+09f;
		}
		public float NextFloat(float a, float b)
		{
			return a + this.NextFloat() * (b - a);
		}
		// Note: this type is marked as 'beforefieldinit'.
		static RandomGen()
		{
			uint?[] array = new uint?[128];
			array[48] = new uint?(0U);
			array[50] = new uint?(1U);
			array[51] = new uint?(2U);
			array[52] = new uint?(3U);
			array[53] = new uint?(4U);
			array[54] = new uint?(5U);
			array[55] = new uint?(6U);
			array[56] = new uint?(7U);
			array[57] = new uint?(8U);
			array[65] = new uint?(9U);
			array[66] = new uint?(10U);
			array[67] = new uint?(11U);
			array[68] = new uint?(12U);
			array[69] = new uint?(13U);
			array[70] = new uint?(14U);
			array[71] = new uint?(15U);
			array[72] = new uint?(16U);
			array[74] = new uint?(17U);
			array[75] = new uint?(18U);
			array[77] = new uint?(19U);
			array[78] = new uint?(20U);
			array[80] = new uint?(21U);
			array[81] = new uint?(22U);
			array[82] = new uint?(23U);
			array[83] = new uint?(24U);
			array[84] = new uint?(25U);
			array[85] = new uint?(26U);
			array[86] = new uint?(27U);
			array[87] = new uint?(28U);
			array[88] = new uint?(29U);
			array[89] = new uint?(30U);
			array[90] = new uint?(31U);
			array[97] = new uint?(9U);
			array[98] = new uint?(10U);
			array[99] = new uint?(11U);
			array[100] = new uint?(12U);
			array[101] = new uint?(13U);
			array[102] = new uint?(14U);
			array[103] = new uint?(15U);
			array[104] = new uint?(16U);
			array[106] = new uint?(17U);
			array[107] = new uint?(18U);
			array[109] = new uint?(19U);
			array[110] = new uint?(20U);
			array[112] = new uint?(21U);
			array[113] = new uint?(22U);
			array[114] = new uint?(23U);
			array[115] = new uint?(24U);
			array[116] = new uint?(25U);
			array[117] = new uint?(26U);
			array[118] = new uint?(27U);
			array[119] = new uint?(28U);
			array[120] = new uint?(29U);
			array[121] = new uint?(30U);
			array[122] = new uint?(31U);
			RandomGen.ReverseTable = array;
			RandomGen.Base = (uint)RandomGen.ForwardTable.Length;
			RandomGen.MaxSeedSize = 13;
		}
		private static readonly RandomGen InitGen = new RandomGen((ulong)DateTime.Now.Ticks);
		private static readonly char[] ForwardTable = new char[]
		{
			'0', '2', '3', '4', '5', '6', '7', '8', '9', 'a',
			'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k', 'm',
			'n', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
			'y', 'z'
		};
		private static readonly uint?[] ReverseTable;
		private static readonly uint Base;
		public static readonly int MaxSeedSize;
		private const ulong Multiplier = 6364136223846793005UL;
		private const ulong Increment = 1442695040888963407UL;
	}
}
