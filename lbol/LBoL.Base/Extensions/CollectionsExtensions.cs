using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LBoL.Base.Extensions
{
	// Token: 0x02000020 RID: 32
	public static class CollectionsExtensions
	{
		// Token: 0x060000E0 RID: 224 RVA: 0x000055A8 File Offset: 0x000037A8
		[MethodImpl(256)]
		public static bool Empty<T>(this IReadOnlyCollection<T> collection)
		{
			return collection.Count == 0;
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x000055B3 File Offset: 0x000037B3
		[MethodImpl(256)]
		public static bool NotEmpty<T>(this IReadOnlyCollection<T> collection)
		{
			return collection.Count > 0;
		}

		// Token: 0x060000E2 RID: 226 RVA: 0x000055BE File Offset: 0x000037BE
		[return: TupleElementNames(new string[] { "index", "elem" })]
		public static IEnumerable<ValueTuple<int, T>> WithIndices<T>(this IEnumerable<T> source)
		{
			int i = 0;
			foreach (T t in source)
			{
				int num = i;
				i = num + 1;
				yield return new ValueTuple<int, T>(num, t);
			}
			IEnumerator<T> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060000E3 RID: 227 RVA: 0x000055D0 File Offset: 0x000037D0
		public static void Shuffle<T>(this IList<T> list, Func<int, int, int> rand)
		{
			int count = list.Count;
			while (count-- > 0)
			{
				int num = rand.Invoke(0, count);
				int num2 = num;
				int num3 = count;
				T t = list[count];
				T t2 = list[num];
				list[num2] = t;
				list[num3] = t2;
			}
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x00005629 File Offset: 0x00003829
		public static void Shuffle<T>(this IList<T> list, RandomGen rand)
		{
			list.Shuffle(new Func<int, int, int>(rand.NextInt));
		}

		// Token: 0x060000E5 RID: 229 RVA: 0x00005640 File Offset: 0x00003840
		[return: MaybeNull]
		public static T SampleOrDefault<T>(this IEnumerable<T> source, Func<int, int, int> rand)
		{
			IReadOnlyList<T> readOnlyList = source as IReadOnlyList<T>;
			if (readOnlyList == null)
			{
				T t;
				using (IEnumerator<T> enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
					{
						t = default(T);
						t = t;
					}
					else
					{
						T t2 = enumerator.Current;
						int num = 0;
						while (enumerator.MoveNext())
						{
							num++;
							if (rand.Invoke(0, num) == 0)
							{
								t2 = enumerator.Current;
							}
						}
						t = t2;
					}
				}
				return t;
			}
			if (readOnlyList.Count == 0)
			{
				T t = default(T);
				return t;
			}
			return readOnlyList[rand.Invoke(0, readOnlyList.Count - 1)];
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x000056E8 File Offset: 0x000038E8
		[return: MaybeNull]
		public static T SampleOrDefault<T>(this IEnumerable<T> source, RandomGen rand)
		{
			return source.SampleOrDefault(new Func<int, int, int>(rand.NextInt));
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x000056FC File Offset: 0x000038FC
		public static T Sample<T>(this IEnumerable<T> source, Func<int, int, int> rand)
		{
			IReadOnlyList<T> readOnlyList = source as IReadOnlyList<T>;
			if (readOnlyList == null)
			{
				T t2;
				using (IEnumerator<T> enumerator = source.GetEnumerator())
				{
					if (!enumerator.MoveNext())
					{
						throw new ArgumentException("Cannot sample from empty source");
					}
					T t = enumerator.Current;
					int num = 0;
					while (enumerator.MoveNext())
					{
						num++;
						if (rand.Invoke(0, num) == 0)
						{
							t = enumerator.Current;
						}
					}
					t2 = t;
				}
				return t2;
			}
			if (readOnlyList.Count == 0)
			{
				throw new ArgumentException("Cannot sample from empty source");
			}
			return readOnlyList[rand.Invoke(0, readOnlyList.Count - 1)];
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x000057A0 File Offset: 0x000039A0
		public static T Sample<T>(this IEnumerable<T> source, RandomGen rand)
		{
			return source.Sample(new Func<int, int, int>(rand.NextInt));
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x000057B4 File Offset: 0x000039B4
		public static T[] SampleManyOrAll<T>(this IEnumerable<T> source, int count, Func<int, int, int> rand)
		{
			if (count < 0)
			{
				throw new ArgumentException("Count for SampleMany should not be negative", "count");
			}
			if (count == 0)
			{
				return Array.Empty<T>();
			}
			T[] array = new T[count];
			T[] array2;
			using (IEnumerator<T> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					array2 = Array.Empty<T>();
				}
				else
				{
					array[0] = enumerator.Current;
					for (int i = 1; i < count; i++)
					{
						if (!enumerator.MoveNext())
						{
							return Enumerable.ToArray<T>(Enumerable.Take<T>(array, i));
						}
						array[i] = enumerator.Current;
						int num = rand.Invoke(0, i);
						if (i != num)
						{
							T[] array3 = array;
							int num2 = i;
							T[] array4 = array;
							int num3 = num;
							T t = array[num];
							T t2 = array[i];
							array3[num2] = t;
							array4[num3] = t2;
						}
					}
					int num4 = count;
					while (enumerator.MoveNext())
					{
						int num5 = rand.Invoke(0, num4);
						if (num5 < count)
						{
							array[num5] = enumerator.Current;
						}
						num4++;
					}
					array2 = array;
				}
			}
			return array2;
		}

		// Token: 0x060000EA RID: 234 RVA: 0x000058DC File Offset: 0x00003ADC
		public static T[] SampleManyOrAll<T>(this IEnumerable<T> source, int count, RandomGen rng)
		{
			return source.SampleManyOrAll(count, new Func<int, int, int>(rng.NextInt));
		}

		// Token: 0x060000EB RID: 235 RVA: 0x000058F4 File Offset: 0x00003AF4
		public static T[] SampleMany<T>(this IEnumerable<T> source, int count, Func<int, int, int> rand)
		{
			if (count < 0)
			{
				throw new ArgumentException("Count for SampleMany should not be negative", "count");
			}
			if (count == 0)
			{
				return Array.Empty<T>();
			}
			T[] array = new T[count];
			T[] array3;
			using (IEnumerator<T> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					throw new ArgumentException(string.Format("Elements not enough for SampleMany({0})", count));
				}
				array[0] = enumerator.Current;
				for (int i = 1; i < count; i++)
				{
					if (!enumerator.MoveNext())
					{
						throw new ArgumentException(string.Format("Elements not enough for SampleMany({0})", count));
					}
					array[i] = enumerator.Current;
					int num = rand.Invoke(0, i);
					if (i != num)
					{
						T[] array2 = array;
						int num2 = i;
						array3 = array;
						int num3 = num;
						T t = array[num];
						T t2 = array[i];
						array2[num2] = t;
						array3[num3] = t2;
					}
				}
				int num4 = count;
				while (enumerator.MoveNext())
				{
					int num5 = rand.Invoke(0, num4);
					if (num5 < count)
					{
						array[num5] = enumerator.Current;
					}
					num4++;
				}
				array3 = array;
			}
			return array3;
		}

		// Token: 0x060000EC RID: 236 RVA: 0x00005A24 File Offset: 0x00003C24
		public static T[] SampleMany<T>(this IEnumerable<T> source, int count, RandomGen rng)
		{
			return source.SampleMany(count, new Func<int, int, int>(rng.NextInt));
		}

		// Token: 0x060000ED RID: 237 RVA: 0x00005A3C File Offset: 0x00003C3C
		public static int IndexOf<T>(this IReadOnlyList<T> list, T elem)
		{
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			for (int i = 0; i < list.Count; i++)
			{
				if (@default.Equals(list[i], elem))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x00005A74 File Offset: 0x00003C74
		public static int FindIndexOf<T>(this IReadOnlyList<T> list, Predicate<T> predicator)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (predicator.Invoke(list[i]))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060000EF RID: 239 RVA: 0x00005AA4 File Offset: 0x00003CA4
		public static T TryGetValue<T>(this T[] array, int index)
		{
			if (index < 0 || index >= array.Length)
			{
				return default(T);
			}
			return array[index];
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00005ACC File Offset: 0x00003CCC
		public static bool TryGetValue<T>(this T[] array, int index, out T result)
		{
			if (index >= 0 && index < array.Length)
			{
				result = array[index];
				return true;
			}
			result = default(T);
			return false;
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x00005AEF File Offset: 0x00003CEF
		public static void TrySetValue<T>(this T[] array, int index, T value)
		{
			if (index >= 0 && index < array.Length)
			{
				array[index] = value;
			}
		}

		// Token: 0x060000F2 RID: 242 RVA: 0x00005B04 File Offset: 0x00003D04
		public static T TryGetValue<T>(this IReadOnlyList<T> list, int index)
		{
			if (index < 0 || index >= list.Count)
			{
				return default(T);
			}
			return list[index];
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x00005B2F File Offset: 0x00003D2F
		public static bool TryGetValue<T>(this IReadOnlyList<T> list, int index, out T result)
		{
			if (index >= 0 && index < list.Count)
			{
				result = list[index];
				return true;
			}
			result = default(T);
			return false;
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00005B55 File Offset: 0x00003D55
		public static void TrySetValue<T>(this IList<T> list, int index, T value)
		{
			if (index >= 0 && index < list.Count)
			{
				list[index] = value;
			}
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00005B6C File Offset: 0x00003D6C
		public static IEnumerable<ValueTuple<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> seq1, IEnumerable<T2> seq2)
		{
			CollectionsExtensions.<Zip>d__21<T1, T2> <Zip>d__ = new CollectionsExtensions.<Zip>d__21<T1, T2>(-2);
			<Zip>d__.<>3__seq1 = seq1;
			<Zip>d__.<>3__seq2 = seq2;
			return <Zip>d__;
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00005B83 File Offset: 0x00003D83
		public static IEnumerable<ValueTuple<T1, T2, T3>> Zip<T1, T2, T3>(this IEnumerable<T1> seq1, IEnumerable<T2> seq2, IEnumerable<T3> seq3)
		{
			CollectionsExtensions.<Zip>d__22<T1, T2, T3> <Zip>d__ = new CollectionsExtensions.<Zip>d__22<T1, T2, T3>(-2);
			<Zip>d__.<>3__seq1 = seq1;
			<Zip>d__.<>3__seq2 = seq2;
			<Zip>d__.<>3__seq3 = seq3;
			return <Zip>d__;
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00005BA1 File Offset: 0x00003DA1
		public static IEnumerable<int> AsEnumerable(this Range range)
		{
			if (range.Start.IsFromEnd || range.End.IsFromEnd)
			{
				throw new ArgumentException(string.Format("Cannot enumerate range {0}", range));
			}
			int value = range.Start.Value;
			int num = range.End.Value;
			int num2 = value;
			int end = num;
			if (num2 > end)
			{
				throw new ArgumentException(string.Format("Cannot enumerate range {0}", range));
			}
			for (int elem = num2; elem < end; elem = num)
			{
				yield return elem;
				num = elem + 1;
			}
			yield break;
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00005BB1 File Offset: 0x00003DB1
		public static IEnumerable<ValueTuple<int, int>> Zip(this Range range1, Range range2)
		{
			if (range1.Start.IsFromEnd || range2.Start.IsFromEnd || range1.End.IsFromEnd || range2.End.IsFromEnd)
			{
				throw new ArgumentException(string.Format("Cannot zip range {0} and {1}", range1, range2));
			}
			int value = range1.Start.Value;
			int value2 = range2.Start.Value;
			int num = range1.End.Value;
			int value3 = range2.End.Value;
			int start = value;
			int start2 = value2;
			int num2 = num;
			int num3 = value3;
			if (start > num2 || start2 > num3)
			{
				throw new ArgumentException(string.Format("Cannot zip range {0} and {1}", range1, range2));
			}
			int count = Math.Min(num2 - start, num3 - start2);
			for (int i = 0; i < count; i = num)
			{
				yield return new ValueTuple<int, int>(start + i, start2 + i);
				num = i + 1;
			}
			yield break;
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00005BC8 File Offset: 0x00003DC8
		private static T ExtremaByEnumerator<T, TKey>(IEnumerable<T> source, Func<T, TKey> selector, Func<TKey, TKey, int> comparer, bool returnDefault)
		{
			T t;
			using (IEnumerator<T> enumerator = source.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					if (!returnDefault)
					{
						throw new InvalidOperationException("Sequence is empty");
					}
					t = default(T);
					t = t;
				}
				else
				{
					T t2 = enumerator.Current;
					TKey tkey = selector.Invoke(t2);
					while (enumerator.MoveNext())
					{
						T t3 = enumerator.Current;
						TKey tkey2 = selector.Invoke(t3);
						if (comparer.Invoke(tkey, tkey2) < 0)
						{
							t2 = t3;
							tkey = tkey2;
						}
					}
					t = t2;
				}
			}
			return t;
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00005C5C File Offset: 0x00003E5C
		private static T ExtremaBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, Func<TKey, TKey, int> comparer, bool returnDefault)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (selector == null)
			{
				throw new ArgumentNullException("selector");
			}
			if (comparer == null)
			{
				throw new ArgumentNullException("comparer");
			}
			return CollectionsExtensions.ExtremaByEnumerator<T, TKey>(source, selector, comparer, returnDefault);
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00005C91 File Offset: 0x00003E91
		public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			return source.ExtremaBy(selector, new Func<TKey, TKey, int>(comparer.Compare), false);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00005CA8 File Offset: 0x00003EA8
		public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
		{
			return source.MaxBy(selector, Comparer<TKey>.Default);
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00005CB6 File Offset: 0x00003EB6
		public static T MaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			return source.ExtremaBy(selector, new Func<TKey, TKey, int>(comparer.Compare), true);
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00005CCD File Offset: 0x00003ECD
		public static T MaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
		{
			return source.MaxByOrDefault(selector, Comparer<TKey>.Default);
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00005CDC File Offset: 0x00003EDC
		public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			return source.ExtremaBy(selector, (TKey a, TKey b) => -Math.Sign(comparer.Compare(a, b)), false);
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00005D0A File Offset: 0x00003F0A
		public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
		{
			return source.MinBy(selector, Comparer<TKey>.Default);
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00005D18 File Offset: 0x00003F18
		public static T MinByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			return source.ExtremaBy(selector, (TKey a, TKey b) => -Math.Sign(comparer.Compare(a, b)), true);
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00005D46 File Offset: 0x00003F46
		public static T MinByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
		{
			return source.MinByOrDefault(selector, Comparer<TKey>.Default);
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00005D54 File Offset: 0x00003F54
		public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
		{
			return Array.AsReadOnly<T>(array);
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00005D5C File Offset: 0x00003F5C
		public static int LowerBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, IComparer<TKey> keyComparer)
		{
			int num = 0;
			int num2 = source.Count;
			while (num != num2)
			{
				int num3 = (num + num2) / 2;
				if (keyComparer.Compare(key, keySelector.Invoke(source[num3])) <= 0)
				{
					num2 = num3;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return num;
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00005D9E File Offset: 0x00003F9E
		public static int LowerBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, Comparison<TKey> keyComparison)
		{
			return source.LowerBound(key, keySelector, Comparer<TKey>.Create(keyComparison));
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00005DAE File Offset: 0x00003FAE
		public static int LowerBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector)
		{
			return source.LowerBound(key, keySelector, Comparer<TKey>.Default);
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00005DC0 File Offset: 0x00003FC0
		public static int LowerBound<T>(this IReadOnlyList<T> source, T value, IComparer<T> comparer)
		{
			int num = 0;
			int num2 = source.Count;
			while (num != num2)
			{
				int num3 = (num + num2) / 2;
				if (comparer.Compare(value, source[num3]) <= 0)
				{
					num2 = num3;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return num;
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00005DFC File Offset: 0x00003FFC
		public static int LowerBound<T>(this IReadOnlyList<T> source, T value, Comparison<T> comparison)
		{
			return source.LowerBound(value, Comparer<T>.Create(comparison));
		}

		// Token: 0x06000109 RID: 265 RVA: 0x00005E0B File Offset: 0x0000400B
		public static int LowerBound<T>(this IReadOnlyList<T> source, T value)
		{
			return source.LowerBound(value, Comparer<T>.Default);
		}

		// Token: 0x0600010A RID: 266 RVA: 0x00005E1C File Offset: 0x0000401C
		public static int UpperBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, IComparer<TKey> comparer)
		{
			int num = 0;
			int num2 = source.Count;
			while (num != num2)
			{
				int num3 = (num + num2) / 2;
				if (comparer.Compare(key, keySelector.Invoke(source[num3])) < 0)
				{
					num2 = num3;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return num;
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00005E5E File Offset: 0x0000405E
		public static int UpperBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, Comparison<TKey> comparison)
		{
			return source.UpperBound(key, keySelector, Comparer<TKey>.Create(comparison));
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00005E6E File Offset: 0x0000406E
		public static int UpperBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector)
		{
			return source.UpperBound(key, keySelector, Comparer<TKey>.Default);
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00005E80 File Offset: 0x00004080
		public static int UpperBound<T>(this IReadOnlyList<T> source, T value, IComparer<T> comparer)
		{
			int num = 0;
			int num2 = source.Count;
			while (num != num2)
			{
				int num3 = (num + num2) / 2;
				if (comparer.Compare(value, source[num3]) < 0)
				{
					num2 = num3;
				}
				else
				{
					num = num3 + 1;
				}
			}
			return num;
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00005EBC File Offset: 0x000040BC
		public static int UpperBound<T>(this IReadOnlyList<T> source, T value, Comparison<T> comparison)
		{
			return source.UpperBound(value, Comparer<T>.Create(comparison));
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00005ECB File Offset: 0x000040CB
		public static int UpperBound<T>(this IReadOnlyList<T> source, T value)
		{
			return source.UpperBound(value, Comparer<T>.Default);
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00005ED9 File Offset: 0x000040D9
		public static Range EqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, IComparer<TKey> comparer)
		{
			return new Range(source.LowerBound(key, keySelector, comparer), source.UpperBound(key, keySelector, comparer));
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00005EFC File Offset: 0x000040FC
		public static Range EqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, Comparison<TKey> comparison)
		{
			return new Range(source.LowerBound(key, keySelector, comparison), source.UpperBound(key, keySelector, comparison));
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00005F1F File Offset: 0x0000411F
		public static Range EqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector)
		{
			return source.EqualRange(key, keySelector, Comparer<TKey>.Default);
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00005F2E File Offset: 0x0000412E
		public static Range EqualRange<T>(this IReadOnlyList<T> source, T item, IComparer<T> comparer)
		{
			return new Range(source.LowerBound(item, comparer), source.UpperBound(item, comparer));
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00005F4F File Offset: 0x0000414F
		public static Range EqualRange<T>(this IReadOnlyList<T> source, T item, Comparison<T> comparison)
		{
			return new Range(source.LowerBound(item, comparison), source.UpperBound(item, comparison));
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00005F70 File Offset: 0x00004170
		public static Range EqualRange<T>(this IReadOnlyList<T> source, T item)
		{
			return source.EqualRange(item, Comparer<T>.Default);
		}

		// Token: 0x06000116 RID: 278 RVA: 0x00005F7E File Offset: 0x0000417E
		public static IEnumerable<T> EnumerateEqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, IComparer<TKey> comparer)
		{
			using (BasicTypeExtensions.RangeEnumerator enumerator = source.EqualRange(key, keySelector, comparer).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int num = enumerator.Current;
					yield return source[num];
				}
			}
			yield break;
			yield break;
		}

		// Token: 0x06000117 RID: 279 RVA: 0x00005FA3 File Offset: 0x000041A3
		public static IEnumerable<T> EnumerateEqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, Comparison<TKey> comparison)
		{
			return source.EnumerateEqualRange(key, keySelector, Comparer<TKey>.Create(comparison));
		}

		// Token: 0x06000118 RID: 280 RVA: 0x00005FB3 File Offset: 0x000041B3
		public static IEnumerable<T> EnumerateEqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector)
		{
			return source.EnumerateEqualRange(key, keySelector, Comparer<TKey>.Default);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x00005FC2 File Offset: 0x000041C2
		public static IEnumerable<T> EnumerateEqualRange<T>(this IReadOnlyList<T> source, T item, IComparer<T> comparer)
		{
			using (BasicTypeExtensions.RangeEnumerator enumerator = source.EqualRange(item, comparer).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					int num = enumerator.Current;
					yield return source[num];
				}
			}
			yield break;
			yield break;
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00005FE0 File Offset: 0x000041E0
		public static IEnumerable<T> EnumerateEqualRange<T>(this IReadOnlyList<T> source, T item, Comparison<T> comparison)
		{
			return source.EnumerateEqualRange(item, Comparer<T>.Create(comparison));
		}

		// Token: 0x0600011B RID: 283 RVA: 0x00005FEF File Offset: 0x000041EF
		public static IEnumerable<T> EnumerateEqualRange<T>(this IReadOnlyList<T> source, T item)
		{
			return source.EnumerateEqualRange(item, Comparer<T>.Default);
		}
	}
}
