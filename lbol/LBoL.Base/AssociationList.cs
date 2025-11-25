using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace LBoL.Base
{
	[Serializable]
	public sealed class AssociationList<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	{
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return Enumerable.Select<AssociationList<TKey, TValue>.Entry, KeyValuePair<TKey, TValue>>(this.entries, (AssociationList<TKey, TValue>.Entry entry) => new KeyValuePair<TKey, TValue>(entry.key, entry.value)).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.entries.Add(new AssociationList<TKey, TValue>.Entry
			{
				key = item.Key,
				value = item.Value
			});
		}
		public void Clear()
		{
			this.entries.Clear();
		}
		public AssociationList<TKey, TValue>.Entry EntryAt(int index)
		{
			return this.entries[index];
		}
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return Enumerable.Any<AssociationList<TKey, TValue>.Entry>(this.entries, (AssociationList<TKey, TValue>.Entry entry) => this._keyComparer.Equals(entry.key, item.Key) && this._valueComparer.Equals(entry.value, item.Value));
		}
		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (array.Length - arrayIndex < this.entries.Count)
			{
				throw new ArgumentException("Destination array was not long enough.");
			}
			foreach (AssociationList<TKey, TValue>.Entry entry in this.entries)
			{
				array[arrayIndex++] = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
			}
		}
		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			KeyValuePair<TKey, TValue> keyValuePair = item;
			TKey tkey;
			TValue tvalue;
			keyValuePair.Deconstruct(ref tkey, ref tvalue);
			TKey key = tkey;
			TValue value = tvalue;
			return this.entries.RemoveAll((AssociationList<TKey, TValue>.Entry entry) => this._keyComparer.Equals(entry.key, key) && this._valueComparer.Equals(entry.value, value)) > 0;
		}
		public int Count
		{
			get
			{
				return this.entries.Count;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		public void Add(TKey key, TValue value)
		{
			this.entries.Add(new AssociationList<TKey, TValue>.Entry
			{
				key = key,
				value = value
			});
		}
		public bool ContainsKey(TKey key)
		{
			return this.entries.Exists((AssociationList<TKey, TValue>.Entry entry) => this._keyComparer.Equals(entry.key, key));
		}
		public bool Remove(TKey key)
		{
			return this.entries.RemoveAll((AssociationList<TKey, TValue>.Entry entry) => this._keyComparer.Equals(entry.key, key)) > 0;
		}
		public bool TryGetValue(TKey key, out TValue value)
		{
			foreach (AssociationList<TKey, TValue>.Entry entry in this.entries)
			{
				if (this._keyComparer.Equals(entry.key, key))
				{
					value = entry.value;
					return true;
				}
			}
			value = default(TValue);
			return false;
		}
		public TValue this[TKey key]
		{
			get
			{
				foreach (AssociationList<TKey, TValue>.Entry entry in this.entries)
				{
					if (this._keyComparer.Equals(entry.key, key))
					{
						return entry.value;
					}
				}
				throw new KeyNotFoundException(string.Format("Cannot find '{0}' in association-list", key));
			}
			set
			{
				foreach (AssociationList<TKey, TValue>.Entry entry in this.entries)
				{
					if (this._keyComparer.Equals(entry.key, key))
					{
						entry.value = value;
						return;
					}
				}
				this.entries.Add(new AssociationList<TKey, TValue>.Entry
				{
					key = key,
					value = value
				});
			}
		}
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get
			{
				return this.Keys;
			}
		}
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get
			{
				return this.Values;
			}
		}
		public ICollection<TKey> Keys
		{
			get
			{
				return Enumerable.ToList<TKey>(Enumerable.Select<AssociationList<TKey, TValue>.Entry, TKey>(this.entries, (AssociationList<TKey, TValue>.Entry e) => e.key));
			}
		}
		public ICollection<TValue> Values
		{
			get
			{
				return Enumerable.ToList<TValue>(Enumerable.Select<AssociationList<TKey, TValue>.Entry, TValue>(this.entries, (AssociationList<TKey, TValue>.Entry e) => e.value));
			}
		}
		private IEqualityComparer _keyComparer = EqualityComparer<TKey>.Default;
		private IEqualityComparer _valueComparer = EqualityComparer<TValue>.Default;
		[SerializeField]
		internal List<AssociationList<TKey, TValue>.Entry> entries = new List<AssociationList<TKey, TValue>.Entry>();
		[Serializable]
		public sealed class Entry
		{
			public TKey key;
			public TValue value;
		}
	}
}
