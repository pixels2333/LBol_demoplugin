using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.SaveData;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000F3 RID: 243
	public sealed class RepeatableRandomPool<T> : IRandomPool<T>, IEnumerable<RandomPoolEntry<T>>, IEnumerable
	{
		// Token: 0x06000946 RID: 2374 RVA: 0x0001B027 File Offset: 0x00019227
		public RepeatableRandomPool()
		{
			this._entries = new List<RandomPoolEntry<T>>();
		}

		// Token: 0x06000947 RID: 2375 RVA: 0x0001B03A File Offset: 0x0001923A
		public RepeatableRandomPool(RepeatableRandomPool<T> other)
		{
			this._entries = Enumerable.ToList<RandomPoolEntry<T>>(other._entries);
		}

		// Token: 0x06000948 RID: 2376 RVA: 0x0001B053 File Offset: 0x00019253
		public void Add(RandomPoolEntry<T> entry)
		{
			this._entries.Add(entry);
		}

		// Token: 0x06000949 RID: 2377 RVA: 0x0001B061 File Offset: 0x00019261
		public void Add(T elem, float weight = 1f)
		{
			this._entries.Add(new RandomPoolEntry<T>(elem, weight));
		}

		// Token: 0x0600094A RID: 2378 RVA: 0x0001B075 File Offset: 0x00019275
		public void AddRange(IEnumerable<RandomPoolEntry<T>> range)
		{
			this._entries.AddRange(range);
		}

		// Token: 0x0600094B RID: 2379 RVA: 0x0001B084 File Offset: 0x00019284
		public void Remove(T elem)
		{
			this._entries.RemoveAll((RandomPoolEntry<T> entry) => EqualityComparer<T>.Default.Equals(entry.Elem, elem));
		}

		// Token: 0x0600094C RID: 2380 RVA: 0x0001B0B6 File Offset: 0x000192B6
		public void Clear()
		{
			this._entries.Clear();
		}

		// Token: 0x0600094D RID: 2381 RVA: 0x0001B0C3 File Offset: 0x000192C3
		public RepeatableRandomPool<T> Without(T elem)
		{
			RepeatableRandomPool<T> repeatableRandomPool = new RepeatableRandomPool<T>(this);
			repeatableRandomPool.Remove(elem);
			return repeatableRandomPool;
		}

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x0600094E RID: 2382 RVA: 0x0001B0D2 File Offset: 0x000192D2
		public bool IsEmpty
		{
			get
			{
				return this._entries.Empty<RandomPoolEntry<T>>();
			}
		}

		// Token: 0x0600094F RID: 2383 RVA: 0x0001B0DF File Offset: 0x000192DF
		public IEnumerator<RandomPoolEntry<T>> GetEnumerator()
		{
			return this._entries.GetEnumerator();
		}

		// Token: 0x06000950 RID: 2384 RVA: 0x0001B0F1 File Offset: 0x000192F1
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000951 RID: 2385 RVA: 0x0001B0FC File Offset: 0x000192FC
		public T SampleOrDefault(Func<float, float, float> randomFunc)
		{
			if (!this._entries.Empty<RandomPoolEntry<T>>())
			{
				return this.Sample(randomFunc);
			}
			return default(T);
		}

		// Token: 0x06000952 RID: 2386 RVA: 0x0001B127 File Offset: 0x00019327
		public T SampleOrDefault(RandomGen rng)
		{
			return this.SampleOrDefault(new Func<float, float, float>(rng.NextFloat));
		}

		// Token: 0x06000953 RID: 2387 RVA: 0x0001B13C File Offset: 0x0001933C
		public T Sample(Func<float, float, float> randomFunc)
		{
			if (this._entries.Empty<RandomPoolEntry<T>>())
			{
				throw new InvalidOperationException("Cannot sample from empty pool");
			}
			float num = Enumerable.Sum<RandomPoolEntry<T>>(this._entries, (RandomPoolEntry<T> entry) => entry.Weight);
			float num2 = randomFunc.Invoke(0f, num);
			foreach (RandomPoolEntry<T> randomPoolEntry in this._entries)
			{
				if (num2 <= randomPoolEntry.Weight)
				{
					return randomPoolEntry.Elem;
				}
				num2 -= randomPoolEntry.Weight;
			}
			return Enumerable.Last<RandomPoolEntry<T>>(this._entries).Elem;
		}

		// Token: 0x06000954 RID: 2388 RVA: 0x0001B208 File Offset: 0x00019408
		public T Sample(RandomGen rng)
		{
			return this.Sample(new Func<float, float, float>(rng.NextFloat));
		}

		// Token: 0x06000955 RID: 2389 RVA: 0x0001B21C File Offset: 0x0001941C
		public T[] SampleMany(Func<float, float, float> randomFunc, int m, bool ensureCount = true)
		{
			List<T> list = new List<T>();
			if (ensureCount)
			{
				while (m-- > 0)
				{
					list.Add(this.Sample(randomFunc));
				}
			}
			else
			{
				while (m-- > 0 && !this.IsEmpty)
				{
					list.Add(this.Sample(randomFunc));
				}
			}
			return list.ToArray();
		}

		// Token: 0x06000956 RID: 2390 RVA: 0x0001B270 File Offset: 0x00019470
		public T[] SampleMany(RandomGen rng, int m, bool ensureCount = true)
		{
			return this.SampleMany(new Func<float, float, float>(rng.NextFloat), m, ensureCount);
		}

		// Token: 0x06000957 RID: 2391 RVA: 0x0001B288 File Offset: 0x00019488
		public RepeatableRandomPoolSaveData<T2> Save<T2>(Func<T, T2> converter)
		{
			RepeatableRandomPoolSaveData<T2> repeatableRandomPoolSaveData = new RepeatableRandomPoolSaveData<T2>();
			foreach (RandomPoolEntry<T> randomPoolEntry in this._entries)
			{
				repeatableRandomPoolSaveData.Elems.Add(new RandomPoolEntrySaveData<T2>
				{
					Elem = converter.Invoke(randomPoolEntry.Elem),
					Weight = randomPoolEntry.Weight
				});
			}
			return repeatableRandomPoolSaveData;
		}

		// Token: 0x06000958 RID: 2392 RVA: 0x0001B30C File Offset: 0x0001950C
		public static RepeatableRandomPool<T> Restore<T2>(RepeatableRandomPoolSaveData<T2> data, Func<T2, T> converter)
		{
			RepeatableRandomPool<T> repeatableRandomPool = new RepeatableRandomPool<T>();
			foreach (RandomPoolEntrySaveData<T2> randomPoolEntrySaveData in data.Elems)
			{
				T t = converter.Invoke(randomPoolEntrySaveData.Elem);
				if (t != null)
				{
					repeatableRandomPool._entries.Add(new RandomPoolEntry<T>(t, randomPoolEntrySaveData.Weight));
				}
			}
			return repeatableRandomPool;
		}

		// Token: 0x040004F4 RID: 1268
		private readonly List<RandomPoolEntry<T>> _entries;
	}
}
