using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LBoL.Core
{
	// Token: 0x02000060 RID: 96
	public sealed class OrderedList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
	{
		// Token: 0x0600042B RID: 1067 RVA: 0x0000E82A File Offset: 0x0000CA2A
		public OrderedList(IEnumerable<T> source)
			: this(source, Comparer<T>.Default)
		{
		}

		// Token: 0x0600042C RID: 1068 RVA: 0x0000E838 File Offset: 0x0000CA38
		public OrderedList(IEnumerable<T> source, IComparer<T> comparer)
		{
			this._equalityComparer = EqualityComparer<T>.Default;
			this._innerList = new List<T>();
			base..ctor();
			this._comparer = comparer;
			this._innerList.AddRange(Enumerable.OrderBy<T, T>(source, (T t) => t, comparer));
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x0000E899 File Offset: 0x0000CA99
		public OrderedList(IEnumerable<T> source, Comparison<T> comparison)
			: this(source, Comparer<T>.Create(comparison))
		{
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x0000E8A8 File Offset: 0x0000CAA8
		public OrderedList(IComparer<T> comparer)
		{
			this._equalityComparer = EqualityComparer<T>.Default;
			this._innerList = new List<T>();
			base..ctor();
			this._comparer = comparer;
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x0000E8CD File Offset: 0x0000CACD
		public OrderedList(Comparison<T> comparison)
			: this(Comparer<T>.Create(comparison))
		{
		}

		// Token: 0x06000430 RID: 1072 RVA: 0x0000E8DB File Offset: 0x0000CADB
		public OrderedList()
			: this(Comparer<T>.Default)
		{
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x0000E8E8 File Offset: 0x0000CAE8
		private int LowerBound(T item)
		{
			int num = 0;
			int num2 = this._innerList.Count;
			while (num != num2)
			{
				int num3 = (num + num2) / 2;
				if (this._comparer.Compare(item, this._innerList[num3]) <= 0)
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

		// Token: 0x06000432 RID: 1074 RVA: 0x0000E934 File Offset: 0x0000CB34
		private int UpperBound(T item)
		{
			int num = 0;
			int num2 = this._innerList.Count;
			while (num != num2)
			{
				int num3 = (num + num2) / 2;
				if (this._comparer.Compare(item, this._innerList[num3]) < 0)
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

		// Token: 0x06000433 RID: 1075 RVA: 0x0000E97F File Offset: 0x0000CB7F
		private ValueTuple<int, int> EqualRange(T item)
		{
			return new ValueTuple<int, int>(this.LowerBound(item), this.UpperBound(item));
		}

		// Token: 0x06000434 RID: 1076 RVA: 0x0000E994 File Offset: 0x0000CB94
		public IEnumerator<T> GetEnumerator()
		{
			return this._innerList.GetEnumerator();
		}

		// Token: 0x06000435 RID: 1077 RVA: 0x0000E9A6 File Offset: 0x0000CBA6
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000436 RID: 1078 RVA: 0x0000E9B0 File Offset: 0x0000CBB0
		public void Add(T item)
		{
			int num = this.UpperBound(item);
			this._innerList.Insert(num, item);
		}

		// Token: 0x06000437 RID: 1079 RVA: 0x0000E9D2 File Offset: 0x0000CBD2
		public void Clear()
		{
			this._innerList.Clear();
		}

		// Token: 0x06000438 RID: 1080 RVA: 0x0000E9E0 File Offset: 0x0000CBE0
		public int IndexOf(T item)
		{
			ValueTuple<int, int> valueTuple = this.EqualRange(item);
			int item2 = valueTuple.Item1;
			int item3 = valueTuple.Item2;
			for (int i = item2; i < item3; i++)
			{
				if (this._equalityComparer.Equals(this._innerList[i], item))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x0000EA2A File Offset: 0x0000CC2A
		public bool Contains(T item)
		{
			return this.IndexOf(item) >= 0;
		}

		// Token: 0x0600043A RID: 1082 RVA: 0x0000EA39 File Offset: 0x0000CC39
		public void CopyTo(T[] array, int arrayIndex)
		{
			this._innerList.CopyTo(array, arrayIndex);
		}

		// Token: 0x0600043B RID: 1083 RVA: 0x0000EA48 File Offset: 0x0000CC48
		public bool Remove(T item)
		{
			int num = this.IndexOf(item);
			if (num < 0)
			{
				return false;
			}
			this._innerList.RemoveAt(num);
			return true;
		}

		// Token: 0x17000150 RID: 336
		public T this[int index]
		{
			get
			{
				return this._innerList[index];
			}
		}

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x0600043D RID: 1085 RVA: 0x0000EA7E File Offset: 0x0000CC7E
		public int Count
		{
			get
			{
				return this._innerList.Count;
			}
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x0600043E RID: 1086 RVA: 0x0000EA8B File Offset: 0x0000CC8B
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600043F RID: 1087 RVA: 0x0000EA8E File Offset: 0x0000CC8E
		public IReadOnlyList<T> AsReadOnly()
		{
			return this._innerList.AsReadOnly();
		}

		// Token: 0x04000240 RID: 576
		private readonly IComparer<T> _comparer;

		// Token: 0x04000241 RID: 577
		private readonly IEqualityComparer<T> _equalityComparer;

		// Token: 0x04000242 RID: 578
		private readonly List<T> _innerList;
	}
}
