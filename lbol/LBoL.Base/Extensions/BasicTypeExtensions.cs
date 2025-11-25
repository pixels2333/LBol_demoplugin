using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace LBoL.Base.Extensions
{
	public static class BasicTypeExtensions
	{
		public static void Times(this int times, Action action)
		{
			for (int i = 0; i < times; i++)
			{
				action.Invoke();
			}
		}
		public static void Times(this int times, Action<int> action)
		{
			for (int i = 0; i < times; i++)
			{
				action.Invoke(i);
			}
		}
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
		[MethodImpl(256)]
		public static bool IsNaN(this float value)
		{
			return float.IsNaN(value);
		}
		[MethodImpl(256)]
		public static bool IsInfinity(this float value)
		{
			return float.IsInfinity(value);
		}
		[MethodImpl(256)]
		public static bool IsPositiveInfinity(this float value)
		{
			return float.IsPositiveInfinity(value);
		}
		[MethodImpl(256)]
		public static bool IsNegativeInfinity(this float value)
		{
			return float.IsNegativeInfinity(value);
		}
		[MethodImpl(256)]
		public static bool IsNullOrEmpty(this string src)
		{
			return string.IsNullOrEmpty(src);
		}
		[MethodImpl(256)]
		public static bool IsNullOrWhiteSpace(this string src)
		{
			return string.IsNullOrWhiteSpace(src);
		}
		[MethodImpl(256)]
		public static string Join(this string separator, string[] value)
		{
			return string.Join(separator, value);
		}
		[MethodImpl(256)]
		public static string Join<T>(this string separator, IEnumerable<T> value)
		{
			return string.Join<T>(separator, value);
		}
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
		[MethodImpl(256)]
		public static TimeSpan Hours(this double value)
		{
			return TimeSpan.FromHours(value);
		}
		[MethodImpl(256)]
		public static TimeSpan Hours(this float value)
		{
			return TimeSpan.FromHours((double)value);
		}
		[MethodImpl(256)]
		public static TimeSpan Hours(this int value)
		{
			return TimeSpan.FromHours((double)value);
		}
		[MethodImpl(256)]
		public static TimeSpan Minutes(this double value)
		{
			return TimeSpan.FromMinutes(value);
		}
		[MethodImpl(256)]
		public static TimeSpan Minutes(this float value)
		{
			return TimeSpan.FromMinutes((double)value);
		}
		[MethodImpl(256)]
		public static TimeSpan Minutes(this int value)
		{
			return TimeSpan.FromMinutes((double)value);
		}
		[MethodImpl(256)]
		public static TimeSpan Seconds(this double value)
		{
			return TimeSpan.FromSeconds(value);
		}
		[MethodImpl(256)]
		public static TimeSpan Seconds(this float value)
		{
			return TimeSpan.FromSeconds((double)value);
		}
		[MethodImpl(256)]
		public static TimeSpan Seconds(this int value)
		{
			return TimeSpan.FromSeconds((double)value);
		}
		[MethodImpl(256)]
		public static TimeSpan Milliseconds(this double value)
		{
			return TimeSpan.FromMilliseconds(value);
		}
		[MethodImpl(256)]
		public static TimeSpan Milliseconds(this float value)
		{
			return TimeSpan.FromMilliseconds((double)value);
		}
		[MethodImpl(256)]
		public static TimeSpan Milliseconds(this int value)
		{
			return TimeSpan.FromMilliseconds((double)value);
		}
		public struct RangeEnumerator : IEnumerator<int>, IEnumerator, IDisposable
		{
			public RangeEnumerator(int start, int end)
			{
				this._current = start - 1;
				this._end = end;
			}
			public bool MoveNext()
			{
				this._current++;
				return this._current < this._end;
			}
			public void Reset()
			{
				throw new NotSupportedException();
			}
			public int Current
			{
				get
				{
					return this._current;
				}
			}
			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}
			public void Dispose()
			{
			}
			private int _current;
			private readonly int _end;
		}
	}
}
