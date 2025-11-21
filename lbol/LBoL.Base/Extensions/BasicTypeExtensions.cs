using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LBoL.Base.Extensions
{
	// Token: 0x0200001F RID: 31
	public static class BasicTypeExtensions
	{
		// Token: 0x060000C2 RID: 194 RVA: 0x00005300 File Offset: 0x00003500
		public static void Times(this int times, Action action)
		{
			for (int i = 0; i < times; i++)
			{
				action.Invoke();
			}
		}

		// Token: 0x060000C3 RID: 195 RVA: 0x00005320 File Offset: 0x00003520
		public static void Times(this int times, Action<int> action)
		{
			for (int i = 0; i < times; i++)
			{
				action.Invoke(i);
			}
		}

		// Token: 0x060000C4 RID: 196 RVA: 0x00005340 File Offset: 0x00003540
		public static IEnumerable<T> Times<T>(this int times, Func<T> func)
		{
			int num;
			for (int i = 0; i < times; i = num)
			{
				yield return func.Invoke();
				num = i + 1;
			}
			yield break;
		}

		// Token: 0x060000C5 RID: 197 RVA: 0x00005357 File Offset: 0x00003557
		public static IEnumerable<T> Times<T>(this int times, Func<int, T> func)
		{
			int num;
			for (int i = 0; i < times; i = num)
			{
				yield return func.Invoke(i);
				num = i + 1;
			}
			yield break;
		}

		// Token: 0x060000C6 RID: 198 RVA: 0x00005370 File Offset: 0x00003570
		public static BasicTypeExtensions.RangeEnumerator GetEnumerator(this Range range)
		{
			Index start = range.Start;
			Index end = range.End;
			if (start.IsFromEnd || end.IsFromEnd)
			{
				throw new InvalidOperationException(string.Format("Cannot enumerate range {0} because it contains from-end indices", range));
			}
			return new BasicTypeExtensions.RangeEnumerator(start.Value, end.Value);
		}

		// Token: 0x060000C7 RID: 199 RVA: 0x000053C8 File Offset: 0x000035C8
		public static int Count(this Range range)
		{
			Index start = range.Start;
			Index end = range.End;
			if (start.IsFromEnd || end.IsFromEnd)
			{
				throw new InvalidOperationException(string.Format("Cannot enumerate range {0} because it contains from-end indices", range));
			}
			return end.Value - start.Value;
		}

		// Token: 0x060000C8 RID: 200 RVA: 0x0000541C File Offset: 0x0000361C
		[MethodImpl(256)]
		public static bool IsNaN(this float value)
		{
			return float.IsNaN(value);
		}

		// Token: 0x060000C9 RID: 201 RVA: 0x00005424 File Offset: 0x00003624
		[MethodImpl(256)]
		public static bool IsInfinity(this float value)
		{
			return float.IsInfinity(value);
		}

		// Token: 0x060000CA RID: 202 RVA: 0x0000542C File Offset: 0x0000362C
		[MethodImpl(256)]
		public static bool IsPositiveInfinity(this float value)
		{
			return float.IsPositiveInfinity(value);
		}

		// Token: 0x060000CB RID: 203 RVA: 0x00005434 File Offset: 0x00003634
		[MethodImpl(256)]
		public static bool IsNegativeInfinity(this float value)
		{
			return float.IsNegativeInfinity(value);
		}

		// Token: 0x060000CC RID: 204 RVA: 0x0000543C File Offset: 0x0000363C
		[MethodImpl(256)]
		public static bool IsNullOrEmpty(this string src)
		{
			return string.IsNullOrEmpty(src);
		}

		// Token: 0x060000CD RID: 205 RVA: 0x00005444 File Offset: 0x00003644
		[MethodImpl(256)]
		public static bool IsNullOrWhiteSpace(this string src)
		{
			return string.IsNullOrWhiteSpace(src);
		}

		// Token: 0x060000CE RID: 206 RVA: 0x0000544C File Offset: 0x0000364C
		[MethodImpl(256)]
		public static string Join(this string separator, string[] value)
		{
			return string.Join(separator, value);
		}

		// Token: 0x060000CF RID: 207 RVA: 0x00005455 File Offset: 0x00003655
		[MethodImpl(256)]
		public static string Join<T>(this string separator, IEnumerable<T> value)
		{
			return string.Join<T>(separator, value);
		}

		// Token: 0x060000D0 RID: 208 RVA: 0x00005460 File Offset: 0x00003660
		[MethodImpl(256)]
		public static string RemoveStart(this string src, string start)
		{
			if (!src.StartsWith(start))
			{
				throw new ArgumentException(src + " is not starts with " + start);
			}
			int length = start.Length;
			return src.Substring(length, src.Length - length);
		}

		// Token: 0x060000D1 RID: 209 RVA: 0x000054A0 File Offset: 0x000036A0
		[MethodImpl(256)]
		public static string TryRemoveStart(this string src, string start)
		{
			if (!src.StartsWith(start))
			{
				return src;
			}
			int length = start.Length;
			return src.Substring(length, src.Length - length);
		}

		// Token: 0x060000D2 RID: 210 RVA: 0x000054D0 File Offset: 0x000036D0
		[MethodImpl(256)]
		public static string RemoveEnd(this string src, string end)
		{
			if (!src.EndsWith(end))
			{
				throw new ArgumentException(src + " is not ends with " + end);
			}
			int length = end.Length;
			return src.Substring(0, src.Length - length);
		}

		// Token: 0x060000D3 RID: 211 RVA: 0x00005510 File Offset: 0x00003710
		[MethodImpl(256)]
		public static string TryRemoveEnd(this string src, string end)
		{
			if (!src.EndsWith(end))
			{
				return src;
			}
			int length = end.Length;
			return src.Substring(0, src.Length - length);
		}

		// Token: 0x060000D4 RID: 212 RVA: 0x00005540 File Offset: 0x00003740
		[MethodImpl(256)]
		public static TimeSpan Hours(this double value)
		{
			return TimeSpan.FromHours(value);
		}

		// Token: 0x060000D5 RID: 213 RVA: 0x00005548 File Offset: 0x00003748
		[MethodImpl(256)]
		public static TimeSpan Hours(this float value)
		{
			return TimeSpan.FromHours((double)value);
		}

		// Token: 0x060000D6 RID: 214 RVA: 0x00005551 File Offset: 0x00003751
		[MethodImpl(256)]
		public static TimeSpan Hours(this int value)
		{
			return TimeSpan.FromHours((double)value);
		}

		// Token: 0x060000D7 RID: 215 RVA: 0x0000555A File Offset: 0x0000375A
		[MethodImpl(256)]
		public static TimeSpan Minutes(this double value)
		{
			return TimeSpan.FromMinutes(value);
		}

		// Token: 0x060000D8 RID: 216 RVA: 0x00005562 File Offset: 0x00003762
		[MethodImpl(256)]
		public static TimeSpan Minutes(this float value)
		{
			return TimeSpan.FromMinutes((double)value);
		}

		// Token: 0x060000D9 RID: 217 RVA: 0x0000556B File Offset: 0x0000376B
		[MethodImpl(256)]
		public static TimeSpan Minutes(this int value)
		{
			return TimeSpan.FromMinutes((double)value);
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00005574 File Offset: 0x00003774
		[MethodImpl(256)]
		public static TimeSpan Seconds(this double value)
		{
			return TimeSpan.FromSeconds(value);
		}

		// Token: 0x060000DB RID: 219 RVA: 0x0000557C File Offset: 0x0000377C
		[MethodImpl(256)]
		public static TimeSpan Seconds(this float value)
		{
			return TimeSpan.FromSeconds((double)value);
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00005585 File Offset: 0x00003785
		[MethodImpl(256)]
		public static TimeSpan Seconds(this int value)
		{
			return TimeSpan.FromSeconds((double)value);
		}

		// Token: 0x060000DD RID: 221 RVA: 0x0000558E File Offset: 0x0000378E
		[MethodImpl(256)]
		public static TimeSpan Milliseconds(this double value)
		{
			return TimeSpan.FromMilliseconds(value);
		}

		// Token: 0x060000DE RID: 222 RVA: 0x00005596 File Offset: 0x00003796
		[MethodImpl(256)]
		public static TimeSpan Milliseconds(this float value)
		{
			return TimeSpan.FromMilliseconds((double)value);
		}

		// Token: 0x060000DF RID: 223 RVA: 0x0000559F File Offset: 0x0000379F
		[MethodImpl(256)]
		public static TimeSpan Milliseconds(this int value)
		{
			return TimeSpan.FromMilliseconds((double)value);
		}

		// Token: 0x02000032 RID: 50
		public struct RangeEnumerator : IEnumerator<int>, IEnumerator, IDisposable
		{
			// Token: 0x060001A6 RID: 422 RVA: 0x00006F2B File Offset: 0x0000512B
			public RangeEnumerator(int start, int end)
			{
				this._current = start - 1;
				this._end = end;
			}

			// Token: 0x060001A7 RID: 423 RVA: 0x00006F3D File Offset: 0x0000513D
			public bool MoveNext()
			{
				this._current++;
				return this._current < this._end;
			}

			// Token: 0x060001A8 RID: 424 RVA: 0x00006F5B File Offset: 0x0000515B
			public void Reset()
			{
				throw new NotSupportedException();
			}

			// Token: 0x17000029 RID: 41
			// (get) Token: 0x060001A9 RID: 425 RVA: 0x00006F62 File Offset: 0x00005162
			public int Current
			{
				get
				{
					return this._current;
				}
			}

			// Token: 0x1700002A RID: 42
			// (get) Token: 0x060001AA RID: 426 RVA: 0x00006F6A File Offset: 0x0000516A
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			// Token: 0x060001AB RID: 427 RVA: 0x00006F77 File Offset: 0x00005177
			public void Dispose()
			{
			}

			// Token: 0x040000F0 RID: 240
			private int _current;

			// Token: 0x040000F1 RID: 241
			private readonly int _end;
		}
	}
}
