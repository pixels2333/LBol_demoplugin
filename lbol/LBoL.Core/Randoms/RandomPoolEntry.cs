using System;
namespace LBoL.Core.Randoms
{
	public sealed class RandomPoolEntry<T>
	{
		public RandomPoolEntry(T elem, float weight = 1f)
		{
			this.Elem = elem;
			this.Weight = weight;
		}
		public T Elem { get; }
		public float Weight { get; }
		public void Deconstruct(out T elem, out float weight)
		{
			elem = this.Elem;
			weight = this.Weight;
		}
	}
}
