using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000E2 RID: 226
	internal static class Lzss
	{
		// Token: 0x060008E2 RID: 2274 RVA: 0x00019CF7 File Offset: 0x00017EF7
		public static IEnumerable<byte> Encode(IEnumerable<byte> bytes)
		{
			Lzss.<>c__DisplayClass4_0 CS$<>8__locals1;
			CS$<>8__locals1.lson = new int[4097];
			CS$<>8__locals1.rson = new int[4353];
			CS$<>8__locals1.dad = new int[4097];
			CS$<>8__locals1.matchPosition = 0;
			CS$<>8__locals1.textBuf = new byte[4113];
			byte[] codeBuf = new byte[17];
			for (int j = 4097; j <= 4352; j++)
			{
				CS$<>8__locals1.rson[j] = 4096;
			}
			for (int k = 0; k < 4096; k++)
			{
				CS$<>8__locals1.dad[k] = 4096;
			}
			codeBuf[0] = 0;
			int codeBufPtr;
			byte b = (byte)(codeBufPtr = 1);
			int s = 0;
			int r = 4078;
			using (IEnumerator<byte> enumerator = bytes.GetEnumerator())
			{
				int len = 0;
				while (len < 18 && enumerator.MoveNext())
				{
					CS$<>8__locals1.textBuf[r + len] = enumerator.Current;
					int num = len + 1;
					len = num;
				}
				if (len == 0)
				{
					yield break;
				}
				for (int l = 1; l <= 18; l++)
				{
					Lzss.<Encode>g__InsertNode|4_0(r - l, ref CS$<>8__locals1);
				}
				Lzss.<Encode>g__InsertNode|4_0(r, ref CS$<>8__locals1);
				do
				{
					if (CS$<>8__locals1.matchLength > len)
					{
						CS$<>8__locals1.matchLength = len;
					}
					int num;
					if ((long)CS$<>8__locals1.matchLength <= 2L)
					{
						CS$<>8__locals1.matchLength = 1;
						byte[] array = codeBuf;
						int num2 = 0;
						array[num2] |= b;
						byte[] array2 = codeBuf;
						num = codeBufPtr;
						codeBufPtr = num + 1;
						array2[num] = CS$<>8__locals1.textBuf[r];
					}
					else
					{
						byte[] array3 = codeBuf;
						num = codeBufPtr;
						codeBufPtr = num + 1;
						array3[num] = (byte)CS$<>8__locals1.matchPosition;
						byte[] array4 = codeBuf;
						num = codeBufPtr;
						codeBufPtr = num + 1;
						array4[num] = (byte)(((long)(CS$<>8__locals1.matchPosition >> 4) & 240L) | ((long)CS$<>8__locals1.matchLength - 3L));
					}
					int i;
					if ((b = (byte)(b << 1)) == 0)
					{
						for (i = 0; i < codeBufPtr; i = num)
						{
							yield return codeBuf[i];
							num = i + 1;
						}
						codeBuf[0] = 0;
						b = (byte)(codeBufPtr = 1);
					}
					int matchLength = CS$<>8__locals1.matchLength;
					for (i = 0; i < matchLength; i = num)
					{
						if (!enumerator.MoveNext())
						{
							break;
						}
						byte b2 = enumerator.Current;
						Lzss.<Encode>g__DeleteNode|4_1(s, ref CS$<>8__locals1);
						CS$<>8__locals1.textBuf[s] = b2;
						if (s < 17)
						{
							CS$<>8__locals1.textBuf[s + 4096] = b2;
						}
						s = (s + 1) & 4095;
						r = (r + 1) & 4095;
						Lzss.<Encode>g__InsertNode|4_0(r, ref CS$<>8__locals1);
						num = i + 1;
					}
					for (;;)
					{
						num = i;
						i = num + 1;
						if (num >= matchLength)
						{
							break;
						}
						Lzss.<Encode>g__DeleteNode|4_1(s, ref CS$<>8__locals1);
						s = (s + 1) & 4095;
						r = (r + 1) & 4095;
						num = len - 1;
						len = num;
						if (num > 0)
						{
							Lzss.<Encode>g__InsertNode|4_0(r, ref CS$<>8__locals1);
						}
					}
				}
				while (len > 0);
			}
			IEnumerator<byte> enumerator = null;
			if (codeBufPtr > 1)
			{
				int num;
				for (int len = 0; len < codeBufPtr; len = num)
				{
					yield return codeBuf[len];
					num = len + 1;
				}
			}
			yield break;
			yield break;
		}

		// Token: 0x060008E3 RID: 2275 RVA: 0x00019D07 File Offset: 0x00017F07
		public static IEnumerable<byte> Decode(IEnumerable<byte> bytes)
		{
			Lzss.<Decode>d__5 <Decode>d__ = new Lzss.<Decode>d__5(-2);
			<Decode>d__.<>3__bytes = bytes;
			return <Decode>d__;
		}

		// Token: 0x060008E4 RID: 2276 RVA: 0x00019D18 File Offset: 0x00017F18
		[CompilerGenerated]
		internal static void <Encode>g__InsertNode|4_0(int node, ref Lzss.<>c__DisplayClass4_0 A_1)
		{
			int num = 1;
			int num2 = 4097 + (int)A_1.textBuf[node];
			A_1.rson[node] = (A_1.lson[node] = 4096);
			A_1.matchLength = 0;
			for (;;)
			{
				if (num >= 0)
				{
					if (A_1.rson[num2] == 4096)
					{
						break;
					}
					num2 = A_1.rson[num2];
				}
				else
				{
					if (A_1.lson[num2] == 4096)
					{
						goto IL_007D;
					}
					num2 = A_1.lson[num2];
				}
				int num3 = 1;
				while (num3 < 18 && (num = (int)(A_1.textBuf[node + num3] - A_1.textBuf[num2 + num3])) == 0)
				{
					num3++;
				}
				if (num3 > A_1.matchLength)
				{
					A_1.matchPosition = num2;
					if ((A_1.matchLength = num3) >= 18)
					{
						goto Block_6;
					}
				}
			}
			A_1.rson[num2] = node;
			A_1.dad[node] = num2;
			return;
			IL_007D:
			A_1.lson[num2] = node;
			A_1.dad[node] = num2;
			return;
			Block_6:
			A_1.dad[node] = A_1.dad[num2];
			A_1.lson[node] = A_1.lson[num2];
			A_1.rson[node] = A_1.rson[num2];
			A_1.dad[A_1.lson[num2]] = node;
			A_1.dad[A_1.rson[num2]] = node;
			if (A_1.rson[A_1.dad[num2]] == num2)
			{
				A_1.rson[A_1.dad[num2]] = node;
			}
			else
			{
				A_1.lson[A_1.dad[num2]] = node;
			}
			A_1.dad[num2] = 4096;
		}

		// Token: 0x060008E5 RID: 2277 RVA: 0x00019E98 File Offset: 0x00018098
		[CompilerGenerated]
		internal static void <Encode>g__DeleteNode|4_1(int node, ref Lzss.<>c__DisplayClass4_0 A_1)
		{
			if (A_1.dad[node] == 4096)
			{
				return;
			}
			int num;
			if (A_1.rson[node] == 4096)
			{
				num = A_1.lson[node];
			}
			else if (A_1.lson[node] == 4096)
			{
				num = A_1.rson[node];
			}
			else
			{
				num = A_1.lson[node];
				if (A_1.rson[num] != 4096)
				{
					do
					{
						num = A_1.rson[num];
					}
					while (A_1.rson[num] != 4096);
					A_1.rson[A_1.dad[num]] = A_1.lson[num];
					A_1.dad[A_1.lson[num]] = A_1.dad[num];
					A_1.lson[num] = A_1.lson[node];
					A_1.dad[A_1.lson[node]] = num;
				}
				A_1.rson[num] = A_1.rson[node];
				A_1.dad[A_1.rson[node]] = num;
			}
			A_1.dad[num] = A_1.dad[node];
			if (A_1.rson[A_1.dad[node]] == node)
			{
				A_1.rson[A_1.dad[node]] = num;
			}
			else
			{
				A_1.lson[A_1.dad[node]] = num;
			}
			A_1.dad[node] = 4096;
		}

		// Token: 0x0400047E RID: 1150
		private const int N = 4096;

		// Token: 0x0400047F RID: 1151
		private const int F = 18;

		// Token: 0x04000480 RID: 1152
		private const uint Threshold = 2U;

		// Token: 0x04000481 RID: 1153
		private const int Nil = 4096;
	}
}
