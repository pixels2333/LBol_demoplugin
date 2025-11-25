using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using UnityEngine;
namespace LBoL.Core
{
	internal class FaultTolerantArray<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
	{
		public FaultTolerantArray(IEnumerable<T> elem, T defaultValue, string errorFormatter)
		{
			this._inner = Enumerable.ToArray<T>(elem);
			this._defaultValue = defaultValue;
			this._errorFormatter = errorFormatter;
		}
		public static FaultTolerantArray<T> Empty(T defaultValue, string errorFormatter)
		{
			return new FaultTolerantArray<T>(Enumerable.Empty<T>(), defaultValue, errorFormatter);
		}
		public IEnumerator<T> GetEnumerator()
		{
			return this._inner.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		public void Add(T item)
		{
			throw new NotSupportedException();
		}
		public void Clear()
		{
			throw new NotSupportedException();
		}
		public bool Contains(T item)
		{
			return Enumerable.Contains<T>(this._inner, item);
		}
		public void CopyTo(T[] array, int arrayIndex)
		{
			this._inner.CopyTo(array, arrayIndex);
		}
		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}
		public int Count
		{
			get
			{
				return this._inner.Length;
			}
		}
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}
		public int IndexOf(T item)
		{
			return this._inner.IndexOf(item);
		}
		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}
		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
		public T this[int index]
		{
			get
			{
				if (index >= 0 && index < this._inner.Length)
				{
					return this._inner[index];
				}
				Debug.LogError(string.Format(this._errorFormatter, index));
				return this._defaultValue;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		private readonly T[] _inner;
		private readonly T _defaultValue;
		private readonly string _errorFormatter;
	}
}
