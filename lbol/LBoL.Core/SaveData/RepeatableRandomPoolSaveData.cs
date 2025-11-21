using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000D4 RID: 212
	public sealed class RepeatableRandomPoolSaveData<T>
	{
		// Token: 0x040003FC RID: 1020
		[UsedImplicitly]
		public List<RandomPoolEntrySaveData<T>> Elems = new List<RandomPoolEntrySaveData<T>>();
	}
}
