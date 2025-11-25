using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core.SaveData;
using UnityEngine;
namespace LBoL.Core.Randoms
{
	public sealed class UniqueRandomPool<T> : IRandomPool<T>, IEnumerable<RandomPoolEntry<T>>, IEnumerable
	{
		public UniqueRandomPool(bool fallback = false)
		{
			this._entries = new List<RandomPoolEntry<T>>();
			this._fallbackPool = new RepeatableRandomPool<T>();
			this._fallback = fallback;
		}
		public UniqueRandomPool(UniqueRandomPool<T> other)
		{
			this._entries = Enumerable.ToList<RandomPoolEntry<T>>(other._entries);
			this._fallbackPool = new RepeatableRandomPool<T>(other._fallbackPool);
			this._fallback = other._fallback;
		}
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
		public void Remove(T elem, bool removeFallback = true)
		{
			this._entries.RemoveAll((RandomPoolEntry<T> entry) => EqualityComparer<T>.Default.Equals(entry.Elem, elem));
			if (removeFallback && this._fallback)
			{
				this._fallbackPool.Remove(elem);
			}
		}
		public void Clear()
		{
			this._entries.Clear();
			if (this._fallback)
			{
				this._fallbackPool.Clear();
			}
		}
		public UniqueRandomPool<T> Without(T elem)
		{
			UniqueRandomPool<T> uniqueRandomPool = new UniqueRandomPool<T>(this);
			uniqueRandomPool.Remove(elem, true);
			return uniqueRandomPool;
		}
		public bool IsEmpty
		{
			get
			{
				return this._entries.Count == 0;
			}
		}
		public T SampleOrDefault(Func<float, float, float> randomFunc)
		{
			if (!this.IsEmpty)
			{
				return this.Sample(randomFunc);
			}
			return default(T);
		}
		public T SampleOrDefault(RandomGen rng)
		{
			return this.SampleOrDefault(new Func<float, float, float>(rng.NextFloat));
		}
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
		public T Sample(RandomGen rng)
		{
			return this.Sample(new Func<float, float, float>(rng.NextFloat));
		}
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
		public T[] SampleMany(RandomGen rng, int m, bool ensureCount = true)
		{
			return this.SampleMany(new Func<float, float, float>(rng.NextFloat), m, ensureCount);
		}
		public IEnumerator<RandomPoolEntry<T>> GetEnumerator()
		{
			return this._entries.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
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
		private readonly List<RandomPoolEntry<T>> _entries;
		private readonly RepeatableRandomPool<T> _fallbackPool;
		private readonly bool _fallback;
	}
}
