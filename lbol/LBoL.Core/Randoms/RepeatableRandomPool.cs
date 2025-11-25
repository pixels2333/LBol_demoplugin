using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.SaveData;
namespace LBoL.Core.Randoms
{
	public sealed class RepeatableRandomPool<T> : IRandomPool<T>, IEnumerable<RandomPoolEntry<T>>, IEnumerable
	{
		public RepeatableRandomPool()
		{
			this._entries = new List<RandomPoolEntry<T>>();
		}
		public RepeatableRandomPool(RepeatableRandomPool<T> other)
		{
			this._entries = Enumerable.ToList<RandomPoolEntry<T>>(other._entries);
		}
		public void Add(RandomPoolEntry<T> entry)
		{
			this._entries.Add(entry);
		}
		public void Add(T elem, float weight = 1f)
		{
			this._entries.Add(new RandomPoolEntry<T>(elem, weight));
		}
		public void AddRange(IEnumerable<RandomPoolEntry<T>> range)
		{
			this._entries.AddRange(range);
		}
		public void Remove(T elem)
		{
			this._entries.RemoveAll((RandomPoolEntry<T> entry) => EqualityComparer<T>.Default.Equals(entry.Elem, elem));
		}
		public void Clear()
		{
			this._entries.Clear();
		}
		public RepeatableRandomPool<T> Without(T elem)
		{
			RepeatableRandomPool<T> repeatableRandomPool = new RepeatableRandomPool<T>(this);
			repeatableRandomPool.Remove(elem);
			return repeatableRandomPool;
		}
		public bool IsEmpty
		{
			get
			{
				return this._entries.Empty<RandomPoolEntry<T>>();
			}
		}
		public IEnumerator<RandomPoolEntry<T>> GetEnumerator()
		{
			return this._entries.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public T SampleOrDefault(Func<float, float, float> randomFunc)
		{
			if (!this._entries.Empty<RandomPoolEntry<T>>())
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
		private readonly List<RandomPoolEntry<T>> _entries;
	}
}
