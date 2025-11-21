using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LBoL.Base
{
	// Token: 0x02000018 RID: 24
	public class PriorityQueue<TElement, TPriority>
	{
		// Token: 0x060000A1 RID: 161 RVA: 0x00004850 File Offset: 0x00002A50
		private void Grow(int minCapacity)
		{
			int num = Math.Max(this._nodes.Length * 2, this._nodes.Length + 4);
			if (num < minCapacity)
			{
				num = minCapacity;
			}
			Array.Resize<ValueTuple<TElement, TPriority>>(ref this._nodes, num);
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x00004889 File Offset: 0x00002A89
		public int EnsureCapacity(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException("capacity", capacity, "Cannot set capacity to negative size");
			}
			if (this._nodes.Length < capacity)
			{
				this.Grow(capacity);
			}
			return this._nodes.Length;
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x000048C0 File Offset: 0x00002AC0
		public void TrimExcess()
		{
			int num = (int)((double)this._nodes.Length * 0.9);
			if (this._size < num)
			{
				Array.Resize<ValueTuple<TElement, TPriority>>(ref this._nodes, this._size);
			}
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x000048FC File Offset: 0x00002AFC
		public void Clear()
		{
			if (RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<TElement, TPriority>>())
			{
				Array.Clear(this._nodes, 0, this._size);
			}
			this._size = 0;
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x0000491E File Offset: 0x00002B1E
		[MethodImpl(256)]
		private static int GetParent(int index)
		{
			return index - 1 >> 1;
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x00004925 File Offset: 0x00002B25
		[MethodImpl(256)]
		private static int GetLeft(int index)
		{
			return (index << 1) + 1;
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x0000492C File Offset: 0x00002B2C
		[MethodImpl(256)]
		private static int GetRight(int index)
		{
			return (index << 1) + 2;
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x00004934 File Offset: 0x00002B34
		private void Adjust(int index)
		{
			for (;;)
			{
				int num = index;
				int left = PriorityQueue<TElement, TPriority>.GetLeft(index);
				int right = PriorityQueue<TElement, TPriority>.GetRight(index);
				if (left < this._size && this._comparer.Compare(this._nodes[left].Item2, this._nodes[index].Item2) < 0)
				{
					index = left;
				}
				if (right < this._size && this._comparer.Compare(this._nodes[right].Item2, this._nodes[index].Item2) < 0)
				{
					index = right;
				}
				if (index == num)
				{
					break;
				}
				ref ValueTuple<TElement, TPriority> ptr = ref this._nodes[index];
				ValueTuple<TElement, TPriority>[] nodes = this._nodes;
				int num2 = num;
				ValueTuple<TElement, TPriority> valueTuple = this._nodes[num];
				ValueTuple<TElement, TPriority> valueTuple2 = this._nodes[index];
				ptr = valueTuple;
				nodes[num2] = valueTuple2;
			}
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00004A16 File Offset: 0x00002C16
		public PriorityQueue([MaybeNull] IComparer<TPriority> comparer = null)
		{
			this._comparer = comparer ?? Comparer<TPriority>.Default;
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00004A3C File Offset: 0x00002C3C
		public void Enqueue(TElement element, TPriority priority)
		{
			if (this._nodes.Length == this._size)
			{
				this.Grow(this._size + 1);
			}
			int num = this._size;
			this._size++;
			int parent;
			while (num > 0 && this._comparer.Compare(priority, this._nodes[parent = PriorityQueue<TElement, TPriority>.GetParent(num)].Item2) < 0)
			{
				this._nodes[num] = this._nodes[parent];
				num = parent;
			}
			this._nodes[num] = new ValueTuple<TElement, TPriority>(element, priority);
		}

		// Token: 0x060000AB RID: 171 RVA: 0x00004AD6 File Offset: 0x00002CD6
		public TElement Peek()
		{
			if (this._size == 0)
			{
				throw new InvalidOperationException("Cannot peek empty priority-queue");
			}
			return this._nodes[0].Item1;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x00004AFC File Offset: 0x00002CFC
		public bool TryPeek([MaybeNullWhen(false)] out TElement element, [MaybeNullWhen(false)] out TPriority priority)
		{
			if (this._size == 0)
			{
				element = default(TElement);
				priority = default(TPriority);
				return false;
			}
			ValueTuple<TElement, TPriority> valueTuple = this._nodes[0];
			element = valueTuple.Item1;
			priority = valueTuple.Item2;
			return true;
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004B48 File Offset: 0x00002D48
		public TElement Dequeue()
		{
			if (this._size == 0)
			{
				throw new InvalidOperationException("Cannot dequeue empty priority-queue");
			}
			TElement item = this._nodes[0].Item1;
			this._size--;
			this._nodes[0] = this._nodes[this._size];
			if (RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<TElement, TPriority>>())
			{
				this._nodes[this._size] = default(ValueTuple<TElement, TPriority>);
			}
			this.Adjust(0);
			return item;
		}

		// Token: 0x060000AE RID: 174 RVA: 0x00004BCC File Offset: 0x00002DCC
		public bool TryDequeue([MaybeNullWhen(false)] out TElement element, [MaybeNullWhen(false)] out TPriority priority)
		{
			if (this._size == 0)
			{
				element = default(TElement);
				priority = default(TPriority);
				return false;
			}
			ValueTuple<TElement, TPriority> valueTuple = this._nodes[0];
			element = valueTuple.Item1;
			priority = valueTuple.Item2;
			this._size--;
			this._nodes[0] = this._nodes[this._size];
			if (RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<TElement, TPriority>>())
			{
				this._nodes[this._size] = default(ValueTuple<TElement, TPriority>);
			}
			this.Adjust(0);
			return true;
		}

		// Token: 0x0400009E RID: 158
		private int _size;

		// Token: 0x0400009F RID: 159
		[TupleElementNames(new string[] { "Element", "Priority" })]
		private ValueTuple<TElement, TPriority>[] _nodes = Array.Empty<ValueTuple<TElement, TPriority>>();

		// Token: 0x040000A0 RID: 160
		private readonly IComparer<TPriority> _comparer;
	}
}
