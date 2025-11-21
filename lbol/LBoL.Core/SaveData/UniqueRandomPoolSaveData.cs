using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000D5 RID: 213
	public sealed class UniqueRandomPoolSaveData<T>
	{
		// Token: 0x040003FD RID: 1021
		[UsedImplicitly]
		public List<RandomPoolEntrySaveData<T>> Elems = new List<RandomPoolEntrySaveData<T>>();

		// Token: 0x040003FE RID: 1022
		[UsedImplicitly]
		public List<RandomPoolEntrySaveData<T>> FallbackElems = new List<RandomPoolEntrySaveData<T>>();

		// Token: 0x040003FF RID: 1023
		public bool Fallback;
	}
}
