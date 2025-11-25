using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
namespace LBoL.Base
{
	public class PriorityQueue<TElement, TPriority>
	{
		private void Grow(int minCapacity)
		{
			int num = Math.Max(this._nodes.Length * 2, this._nodes.Length + 4);
			if (num < minCapacity)
			{
				num = minCapacity;
			}
			Array.Resize<ValueTuple<TElement, TPriority>>(ref this._nodes, num);
		}
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
		public void TrimExcess()
		{
			int num = (int)((double)this._nodes.Length * 0.9);
			if (this._size < num)
			{
				Array.Resize<ValueTuple<TElement, TPriority>>(ref this._nodes, this._size);
			}
		}
		public void Clear()
		{
			if (RuntimeHelpers.IsReferenceOrContainsReferences<ValueTuple<TElement, TPriority>>())
			{
				Array.Clear(this._nodes, 0, this._size);
			}
			this._size = 0;
		}
		[MethodImpl(256)]
		private static int GetParent(int index)
		{
			return index - 1 >> 1;
		}
		[MethodImpl(256)]
		private static int GetLeft(int index)
		{
			return (index << 1) + 1;
		}
		[MethodImpl(256)]
		private static int GetRight(int index)
		{
			return (index << 1) + 2;
		}
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
		public PriorityQueue([MaybeNull] IComparer<TPriority> comparer = null)
		{
			this._comparer = comparer ?? Comparer<TPriority>.Default;
		}
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
		public TElement Peek()
		{
			if (this._size == 0)
			{
				throw new InvalidOperationException("Cannot peek empty priority-queue");
			}
			return this._nodes[0].Item1;
		}
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
		private int _size;
		[TupleElementNames(new string[] { "Element", "Priority" })]
		private ValueTuple<TElement, TPriority>[] _nodes = Array.Empty<ValueTuple<TElement, TPriority>>();
		private readonly IComparer<TPriority> _comparer;
	}
}
