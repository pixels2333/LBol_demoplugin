using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LBoL.Base
{
	// Token: 0x02000004 RID: 4
	[Serializable]
	public sealed class AssociationList<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
	{
		// Token: 0x06000003 RID: 3 RVA: 0x000020C0 File Offset: 0x000002C0
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return Enumerable.Select<AssociationList<TKey, TValue>.Entry, KeyValuePair<TKey, TValue>>(this.entries, (AssociationList<TKey, TValue>.Entry entry) => new KeyValuePair<TKey, TValue>(entry.key, entry.value)).GetEnumerator();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000020F1 File Offset: 0x000002F1
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000020F9 File Offset: 0x000002F9
		public void Add(KeyValuePair<TKey, TValue> item)
		{
			this.entries.Add(new AssociationList<TKey, TValue>.Entry
			{
				key = item.Key,
				value = item.Value
			});
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002125 File Offset: 0x00000325
		public void Clear()
		{
			this.entries.Clear();
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002132 File Offset: 0x00000332
		public AssociationList<TKey, TValue>.Entry EntryAt(int index)
		{
			return this.entries[index];
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002140 File Offset: 0x00000340
		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return Enumerable.Any<AssociationList<TKey, TValue>.Entry>(this.entries, (AssociationList<TKey, TValue>.Entry entry) => this._keyComparer.Equals(entry.key, item.Key) && this._valueComparer.Equals(entry.value, item.Value));
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002178 File Offset: 0x00000378
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

		// Token: 0x0600000A RID: 10 RVA: 0x00002200 File Offset: 0x00000400
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

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x0600000B RID: 11 RVA: 0x0000224F File Offset: 0x0000044F
		public int Count
		{
			get
			{
				return this.entries.Count;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600000C RID: 12 RVA: 0x0000225C File Offset: 0x0000045C
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x0000225F File Offset: 0x0000045F
		public void Add(TKey key, TValue value)
		{
			this.entries.Add(new AssociationList<TKey, TValue>.Entry
			{
				key = key,
				value = value
			});
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002280 File Offset: 0x00000480
		public bool ContainsKey(TKey key)
		{
			return this.entries.Exists((AssociationList<TKey, TValue>.Entry entry) => this._keyComparer.Equals(entry.key, key));
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000022B8 File Offset: 0x000004B8
		public bool Remove(TKey key)
		{
			return this.entries.RemoveAll((AssociationList<TKey, TValue>.Entry entry) => this._keyComparer.Equals(entry.key, key)) > 0;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000022F4 File Offset: 0x000004F4
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

		// Token: 0x17000003 RID: 3
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

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000013 RID: 19 RVA: 0x00002498 File Offset: 0x00000698
		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get
			{
				return this.Keys;
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000024A0 File Offset: 0x000006A0
		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get
			{
				return this.Values;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000015 RID: 21 RVA: 0x000024A8 File Offset: 0x000006A8
		public ICollection<TKey> Keys
		{
			get
			{
				return Enumerable.ToList<TKey>(Enumerable.Select<AssociationList<TKey, TValue>.Entry, TKey>(this.entries, (AssociationList<TKey, TValue>.Entry e) => e.key));
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000016 RID: 22 RVA: 0x000024D9 File Offset: 0x000006D9
		public ICollection<TValue> Values
		{
			get
			{
				return Enumerable.ToList<TValue>(Enumerable.Select<AssociationList<TKey, TValue>.Entry, TValue>(this.entries, (AssociationList<TKey, TValue>.Entry e) => e.value));
			}
		}

		// Token: 0x04000006 RID: 6
		private IEqualityComparer _keyComparer = EqualityComparer<TKey>.Default;

		// Token: 0x04000007 RID: 7
		private IEqualityComparer _valueComparer = EqualityComparer<TValue>.Default;

		// Token: 0x04000008 RID: 8
		[SerializeField]
		internal List<AssociationList<TKey, TValue>.Entry> entries = new List<AssociationList<TKey, TValue>.Entry>();

		// Token: 0x02000027 RID: 39
		[Serializable]
		public sealed class Entry
		{
			// Token: 0x040000D4 RID: 212
			public TKey key;

			// Token: 0x040000D5 RID: 213
			public TValue value;
		}
	}
}
