using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using UnityEngine;

namespace LBoL.Core
{
	// Token: 0x0200000F RID: 15
	internal class FaultTolerantArray<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
	{
		// Token: 0x06000093 RID: 147 RVA: 0x00003412 File Offset: 0x00001612
		public FaultTolerantArray(IEnumerable<T> elem, T defaultValue, string errorFormatter)
		{
			this._inner = Enumerable.ToArray<T>(elem);
			this._defaultValue = defaultValue;
			this._errorFormatter = errorFormatter;
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00003434 File Offset: 0x00001634
		public static FaultTolerantArray<T> Empty(T defaultValue, string errorFormatter)
		{
			return new FaultTolerantArray<T>(Enumerable.Empty<T>(), defaultValue, errorFormatter);
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00003442 File Offset: 0x00001642
		public IEnumerator<T> GetEnumerator()
		{
			return this._inner.GetEnumerator();
		}

		// Token: 0x06000096 RID: 150 RVA: 0x0000344F File Offset: 0x0000164F
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00003457 File Offset: 0x00001657
		public void Add(T item)
		{
			throw new NotSupportedException();
		}

		// Token: 0x06000098 RID: 152 RVA: 0x0000345E File Offset: 0x0000165E
		public void Clear()
		{
			throw new NotSupportedException();
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00003465 File Offset: 0x00001665
		public bool Contains(T item)
		{
			return Enumerable.Contains<T>(this._inner, item);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003473 File Offset: 0x00001673
		public void CopyTo(T[] array, int arrayIndex)
		{
			this._inner.CopyTo(array, arrayIndex);
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00003482 File Offset: 0x00001682
		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600009C RID: 156 RVA: 0x00003489 File Offset: 0x00001689
		public int Count
		{
			get
			{
				return this._inner.Length;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600009D RID: 157 RVA: 0x00003493 File Offset: 0x00001693
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00003496 File Offset: 0x00001696
		public int IndexOf(T item)
		{
			return this._inner.IndexOf(item);
		}

		// Token: 0x0600009F RID: 159 RVA: 0x000034A4 File Offset: 0x000016A4
		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x000034AB File Offset: 0x000016AB
		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		// Token: 0x1700002C RID: 44
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

		// Token: 0x0400006F RID: 111
		private readonly T[] _inner;

		// Token: 0x04000070 RID: 112
		private readonly T _defaultValue;

		// Token: 0x04000071 RID: 113
		private readonly string _errorFormatter;
	}
}
