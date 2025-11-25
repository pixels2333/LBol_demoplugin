using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
namespace LBoL.Base.Extensions
{
	public static class CollectionsExtensions
	{
		[MethodImpl(256)]
		public static bool Empty<T>(this IReadOnlyCollection<T> collection)
		{
			return collection.Count == 0;
		}
		[MethodImpl(256)]
		public static bool NotEmpty<T>(this IReadOnlyCollection<T> collection)
		{
			return collection.Count > 0;
		}
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
		public static void Shuffle<T>(this IList<T> list, RandomGen rand)
		{
			list.Shuffle(new Func<int, int, int>(rand.NextInt));
		}
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
		[return: MaybeNull]
		public static T SampleOrDefault<T>(this IEnumerable<T> source, RandomGen rand)
		{
			return source.SampleOrDefault(new Func<int, int, int>(rand.NextInt));
		}
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
		public static T Sample<T>(this IEnumerable<T> source, RandomGen rand)
		{
			return source.Sample(new Func<int, int, int>(rand.NextInt));
		}
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
		public static T[] SampleManyOrAll<T>(this IEnumerable<T> source, int count, RandomGen rng)
		{
			return source.SampleManyOrAll(count, new Func<int, int, int>(rng.NextInt));
		}
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
		public static T[] SampleMany<T>(this IEnumerable<T> source, int count, RandomGen rng)
		{
			return source.SampleMany(count, new Func<int, int, int>(rng.NextInt));
		}
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
		public static T TryGetValue<T>(this T[] array, int index)
		{
			if (index < 0 || index >= array.Length)
			{
				return default(T);
			}
			return array[index];
		}
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
		public static void TrySetValue<T>(this T[] array, int index, T value)
		{
			if (index >= 0 && index < array.Length)
			{
				array[index] = value;
			}
		}
		public static T TryGetValue<T>(this IReadOnlyList<T> list, int index)
		{
			if (index < 0 || index >= list.Count)
			{
				return default(T);
			}
			return list[index];
		}
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
		public static void TrySetValue<T>(this IList<T> list, int index, T value)
		{
			if (index >= 0 && index < list.Count)
			{
				list[index] = value;
			}
		}
		public static IEnumerable<ValueTuple<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> seq1, IEnumerable<T2> seq2)
		{
			CollectionsExtensions.<Zip>d__21<T1, T2> <Zip>d__ = new CollectionsExtensions.<Zip>d__21<T1, T2>(-2);
			<Zip>d__.<>3__seq1 = seq1;
			<Zip>d__.<>3__seq2 = seq2;
			return <Zip>d__;
		}
		public static IEnumerable<ValueTuple<T1, T2, T3>> Zip<T1, T2, T3>(this IEnumerable<T1> seq1, IEnumerable<T2> seq2, IEnumerable<T3> seq3)
		{
			CollectionsExtensions.<Zip>d__22<T1, T2, T3> <Zip>d__ = new CollectionsExtensions.<Zip>d__22<T1, T2, T3>(-2);
			<Zip>d__.<>3__seq1 = seq1;
			<Zip>d__.<>3__seq2 = seq2;
			<Zip>d__.<>3__seq3 = seq3;
			return <Zip>d__;
		}
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
		public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			return source.ExtremaBy(selector, new Func<TKey, TKey, int>(comparer.Compare), false);
		}
		public static T MaxBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
		{
			return source.MaxBy(selector, Comparer<TKey>.Default);
		}
		public static T MaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			return source.ExtremaBy(selector, new Func<TKey, TKey, int>(comparer.Compare), true);
		}
		public static T MaxByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
		{
			return source.MaxByOrDefault(selector, Comparer<TKey>.Default);
		}
		public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			return source.ExtremaBy(selector, (TKey a, TKey b) => -Math.Sign(comparer.Compare(a, b)), false);
		}
		public static T MinBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
		{
			return source.MinBy(selector, Comparer<TKey>.Default);
		}
		public static T MinByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			return source.ExtremaBy(selector, (TKey a, TKey b) => -Math.Sign(comparer.Compare(a, b)), true);
		}
		public static T MinByOrDefault<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
		{
			return source.MinByOrDefault(selector, Comparer<TKey>.Default);
		}
		public static ReadOnlyCollection<T> AsReadOnly<T>(this T[] array)
		{
			return Array.AsReadOnly<T>(array);
		}
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
		public static int LowerBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, Comparison<TKey> keyComparison)
		{
			return source.LowerBound(key, keySelector, Comparer<TKey>.Create(keyComparison));
		}
		public static int LowerBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector)
		{
			return source.LowerBound(key, keySelector, Comparer<TKey>.Default);
		}
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
		public static int LowerBound<T>(this IReadOnlyList<T> source, T value, Comparison<T> comparison)
		{
			return source.LowerBound(value, Comparer<T>.Create(comparison));
		}
		public static int LowerBound<T>(this IReadOnlyList<T> source, T value)
		{
			return source.LowerBound(value, Comparer<T>.Default);
		}
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
		public static int UpperBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, Comparison<TKey> comparison)
		{
			return source.UpperBound(key, keySelector, Comparer<TKey>.Create(comparison));
		}
		public static int UpperBound<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector)
		{
			return source.UpperBound(key, keySelector, Comparer<TKey>.Default);
		}
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
		public static int UpperBound<T>(this IReadOnlyList<T> source, T value, Comparison<T> comparison)
		{
			return source.UpperBound(value, Comparer<T>.Create(comparison));
		}
		public static int UpperBound<T>(this IReadOnlyList<T> source, T value)
		{
			return source.UpperBound(value, Comparer<T>.Default);
		}
		public static Range EqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, IComparer<TKey> comparer)
		{
			return new Range(source.LowerBound(key, keySelector, comparer), source.UpperBound(key, keySelector, comparer));
		}
		public static Range EqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, Comparison<TKey> comparison)
		{
			return new Range(source.LowerBound(key, keySelector, comparison), source.UpperBound(key, keySelector, comparison));
		}
		public static Range EqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector)
		{
			return source.EqualRange(key, keySelector, Comparer<TKey>.Default);
		}
		public static Range EqualRange<T>(this IReadOnlyList<T> source, T item, IComparer<T> comparer)
		{
			return new Range(source.LowerBound(item, comparer), source.UpperBound(item, comparer));
		}
		public static Range EqualRange<T>(this IReadOnlyList<T> source, T item, Comparison<T> comparison)
		{
			return new Range(source.LowerBound(item, comparison), source.UpperBound(item, comparison));
		}
		public static Range EqualRange<T>(this IReadOnlyList<T> source, T item)
		{
			return source.EqualRange(item, Comparer<T>.Default);
		}
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
		public static IEnumerable<T> EnumerateEqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector, Comparison<TKey> comparison)
		{
			return source.EnumerateEqualRange(key, keySelector, Comparer<TKey>.Create(comparison));
		}
		public static IEnumerable<T> EnumerateEqualRange<T, TKey>(this IReadOnlyList<T> source, TKey key, Func<T, TKey> keySelector)
		{
			return source.EnumerateEqualRange(key, keySelector, Comparer<TKey>.Default);
		}
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
		public static IEnumerable<T> EnumerateEqualRange<T>(this IReadOnlyList<T> source, T item, Comparison<T> comparison)
		{
			return source.EnumerateEqualRange(item, Comparer<T>.Create(comparison));
		}
		public static IEnumerable<T> EnumerateEqualRange<T>(this IReadOnlyList<T> source, T item)
		{
			return source.EnumerateEqualRange(item, Comparer<T>.Default);
		}
	}
}
