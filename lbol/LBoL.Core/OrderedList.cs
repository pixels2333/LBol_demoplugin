using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace LBoL.Core
{
	public sealed class OrderedList<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
	{
		public OrderedList(IEnumerable<T> source)
			: this(source, Comparer<T>.Default)
		{
		}
		public OrderedList(IEnumerable<T> source, IComparer<T> comparer)
		{
			this._equalityComparer = EqualityComparer<T>.Default;
			this._innerList = new List<T>();
			base..ctor();
			this._comparer = comparer;
			this._innerList.AddRange(Enumerable.OrderBy<T, T>(source, (T t) => t, comparer));
		}
		public OrderedList(IEnumerable<T> source, Comparison<T> comparison)
			: this(source, Comparer<T>.Create(comparison))
		{
		}
		public OrderedList(IComparer<T> comparer)
		{
			this._equalityComparer = EqualityComparer<T>.Default;
			this._innerList = new List<T>();
			base..ctor();
			this._comparer = comparer;
		}
		public OrderedList(Comparison<T> comparison)
			: this(Comparer<T>.Create(comparison))
		{
		}
		public OrderedList()
			: this(Comparer<T>.Default)
		{
		}
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
		private ValueTuple<int, int> EqualRange(T item)
		{
			return new ValueTuple<int, int>(this.LowerBound(item), this.UpperBound(item));
		}
		public IEnumerator<T> GetEnumerator()
		{
			return this._innerList.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public void Add(T item)
		{
			int num = this.UpperBound(item);
			this._innerList.Insert(num, item);
		}
		public void Clear()
		{
			this._innerList.Clear();
		}
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
		public bool Contains(T item)
		{
			return this.IndexOf(item) >= 0;
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
			this._innerList.CopyTo(array, arrayIndex);
		}
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
		public T this[int index]
		{
			get
			{
				return this._innerList[index];
			}
		}
		public int Count
		{
			get
			{
				return this._innerList.Count;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		public IReadOnlyList<T> AsReadOnly()
		{
			return this._innerList.AsReadOnly();
		}
		private readonly IComparer<T> _comparer;
		private readonly IEqualityComparer<T> _equalityComparer;
		private readonly List<T> _innerList;
	}
}
