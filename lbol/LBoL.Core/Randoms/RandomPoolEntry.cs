using System;

namespace LBoL.Core.Randoms
{
	// Token: 0x020000F0 RID: 240
	public sealed class RandomPoolEntry<T>
	{
		// Token: 0x06000937 RID: 2359 RVA: 0x0001AD67 File Offset: 0x00018F67
		public RandomPoolEntry(T elem, float weight = 1f)
		{
			this.Elem = elem;
			this.Weight = weight;
		}

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06000938 RID: 2360 RVA: 0x0001AD7D File Offset: 0x00018F7D
		public T Elem { get; }

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06000939 RID: 2361 RVA: 0x0001AD85 File Offset: 0x00018F85
		public float Weight { get; }

		// Token: 0x0600093A RID: 2362 RVA: 0x0001AD8D File Offset: 0x00018F8D
		public void Deconstruct(out T elem, out float weight)
		{
			elem = this.Elem;
			weight = this.Weight;
		}
	}
}
