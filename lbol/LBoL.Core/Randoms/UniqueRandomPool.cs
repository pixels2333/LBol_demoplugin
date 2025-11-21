using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core.SaveData;
using UnityEngine;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000F4 RID: 244
	public sealed class UniqueRandomPool<T> : IRandomPool<T>, IEnumerable<RandomPoolEntry<T>>, IEnumerable
	{
		// Token: 0x06000959 RID: 2393 RVA: 0x0001B38C File Offset: 0x0001958C
		public UniqueRandomPool(bool fallback = false)
		{
			this._entries = new List<RandomPoolEntry<T>>();
			this._fallbackPool = new RepeatableRandomPool<T>();
			this._fallback = fallback;
		}

		// Token: 0x0600095A RID: 2394 RVA: 0x0001B3B1 File Offset: 0x000195B1
		public UniqueRandomPool(UniqueRandomPool<T> other)
		{
			this._entries = Enumerable.ToList<RandomPoolEntry<T>>(other._entries);
			this._fallbackPool = new RepeatableRandomPool<T>(other._fallbackPool);
			this._fallback = other._fallback;
		}

		// Token: 0x0600095B RID: 2395 RVA: 0x0001B3E7 File Offset: 0x000195E7
		public void Add(T elem, float weight = 1f)
		{
			if (weight <= 0f)
			{
				return;
			}
			this._entries.Add(new RandomPoolEntry<T>(elem, weight));
			if (this._fallback)
			{
				this._fallbackPool.Add(new RandomPoolEntry<T>(elem, weight));
			}
		}

		// Token: 0x0600095C RID: 2396 RVA: 0x0001B420 File Offset: 0x00019620
		public void Remove(T elem, bool removeFallback = true)
		{
			this._entries.RemoveAll((RandomPoolEntry<T> entry) => EqualityComparer<T>.Default.Equals(entry.Elem, elem));
			if (removeFallback && this._fallback)
			{
				this._fallbackPool.Remove(elem);
			}
		}

		// Token: 0x0600095D RID: 2397 RVA: 0x0001B46E File Offset: 0x0001966E
		public void Clear()
		{
			this._entries.Clear();
			if (this._fallback)
			{
				this._fallbackPool.Clear();
			}
		}

		// Token: 0x0600095E RID: 2398 RVA: 0x0001B48E File Offset: 0x0001968E
		public UniqueRandomPool<T> Without(T elem)
		{
			UniqueRandomPool<T> uniqueRandomPool = new UniqueRandomPool<T>(this);
			uniqueRandomPool.Remove(elem, true);
			return uniqueRandomPool;
		}

		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x0600095F RID: 2399 RVA: 0x0001B49E File Offset: 0x0001969E
		public bool IsEmpty
		{
			get
			{
				return this._entries.Count == 0;
			}
		}

		// Token: 0x06000960 RID: 2400 RVA: 0x0001B4B0 File Offset: 0x000196B0
		public T SampleOrDefault(Func<float, float, float> randomFunc)
		{
			if (!this.IsEmpty)
			{
				return this.Sample(randomFunc);
			}
			return default(T);
		}

		// Token: 0x06000961 RID: 2401 RVA: 0x0001B4D6 File Offset: 0x000196D6
		public T SampleOrDefault(RandomGen rng)
		{
			return this.SampleOrDefault(new Func<float, float, float>(rng.NextFloat));
		}

		// Token: 0x06000962 RID: 2402 RVA: 0x0001B4EC File Offset: 0x000196EC
		public T Sample(Func<float, float, float> randomFunc)
		{
			if (!this.IsEmpty)
			{
				float num = Enumerable.Sum<RandomPoolEntry<T>>(this._entries, (RandomPoolEntry<T> entry) => entry.Weight);
				float num2 = randomFunc.Invoke(0f, num);
				foreach (RandomPoolEntry<T> randomPoolEntry in this._entries)
				{
					if (num2 <= randomPoolEntry.Weight)
					{
						this._entries.Remove(randomPoolEntry);
						return randomPoolEntry.Elem;
					}
					num2 -= randomPoolEntry.Weight;
				}
				RandomPoolEntry<T> randomPoolEntry2 = Enumerable.Last<RandomPoolEntry<T>>(this._entries);
				this._entries.Remove(randomPoolEntry2);
				return randomPoolEntry2.Elem;
			}
			if (this._fallback)
			{
				Debug.LogWarning("Random sampling from fallback pool [" + string.Join<T>(", ", Enumerable.Select<RandomPoolEntry<T>, T>(this._fallbackPool, (RandomPoolEntry<T> e) => e.Elem)) + "]");
				return this._fallbackPool.Sample(randomFunc);
			}
			throw new InvalidOperationException("Cannot sample from empty pool");
		}

		// Token: 0x06000963 RID: 2403 RVA: 0x0001B630 File Offset: 0x00019830
		public T Sample(RandomGen rng)
		{
			return this.Sample(new Func<float, float, float>(rng.NextFloat));
		}

		// Token: 0x06000964 RID: 2404 RVA: 0x0001B644 File Offset: 0x00019844
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

		// Token: 0x06000965 RID: 2405 RVA: 0x0001B698 File Offset: 0x00019898
		public T[] SampleMany(RandomGen rng, int m, bool ensureCount = true)
		{
			return this.SampleMany(new Func<float, float, float>(rng.NextFloat), m, ensureCount);
		}

		// Token: 0x06000966 RID: 2406 RVA: 0x0001B6AE File Offset: 0x000198AE
		public IEnumerator<RandomPoolEntry<T>> GetEnumerator()
		{
			return this._entries.GetEnumerator();
		}

		// Token: 0x06000967 RID: 2407 RVA: 0x0001B6C0 File Offset: 0x000198C0
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x0001B6C8 File Offset: 0x000198C8
		public UniqueRandomPoolSaveData<T2> Save<T2>(Func<T, T2> converter)
		{
			UniqueRandomPoolSaveData<T2> uniqueRandomPoolSaveData = new UniqueRandomPoolSaveData<T2>
			{
				Fallback = this._fallback
			};
			foreach (RandomPoolEntry<T> randomPoolEntry in this._entries)
			{
				uniqueRandomPoolSaveData.Elems.Add(new RandomPoolEntrySaveData<T2>
				{
					Elem = converter.Invoke(randomPoolEntry.Elem),
					Weight = randomPoolEntry.Weight
				});
			}
			if (this._fallback)
			{
				foreach (RandomPoolEntry<T> randomPoolEntry2 in this._fallbackPool)
				{
					uniqueRandomPoolSaveData.FallbackElems.Add(new RandomPoolEntrySaveData<T2>
					{
						Elem = converter.Invoke(randomPoolEntry2.Elem),
						Weight = randomPoolEntry2.Weight
					});
				}
			}
			return uniqueRandomPoolSaveData;
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x0001B7C4 File Offset: 0x000199C4
		public static UniqueRandomPool<T> Restore<T2>(UniqueRandomPoolSaveData<T2> data, Func<T2, T> converter)
		{
			UniqueRandomPool<T> uniqueRandomPool = new UniqueRandomPool<T>(data.Fallback);
			foreach (RandomPoolEntrySaveData<T2> randomPoolEntrySaveData in data.Elems)
			{
				T t = converter.Invoke(randomPoolEntrySaveData.Elem);
				if (t != null)
				{
					uniqueRandomPool._entries.Add(new RandomPoolEntry<T>(t, randomPoolEntrySaveData.Weight));
				}
			}
			if (data.Fallback)
			{
				foreach (RandomPoolEntrySaveData<T2> randomPoolEntrySaveData2 in data.FallbackElems)
				{
					T t2 = converter.Invoke(randomPoolEntrySaveData2.Elem);
					if (t2 != null)
					{
						uniqueRandomPool._fallbackPool.Add(new RandomPoolEntry<T>(t2, randomPoolEntrySaveData2.Weight));
					}
				}
			}
			return uniqueRandomPool;
		}

		// Token: 0x040004F5 RID: 1269
		private readonly List<RandomPoolEntry<T>> _entries;

		// Token: 0x040004F6 RID: 1270
		private readonly RepeatableRandomPool<T> _fallbackPool;

		// Token: 0x040004F7 RID: 1271
		private readonly bool _fallback;
	}
}
