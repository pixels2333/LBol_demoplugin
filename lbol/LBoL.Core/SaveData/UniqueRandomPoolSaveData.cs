using System;
using System.Collections.Generic;
using JetBrains.Annotations;
namespace LBoL.Core.SaveData
{
	public sealed class UniqueRandomPoolSaveData<T>
	{
		[UsedImplicitly]
		public List<RandomPoolEntrySaveData<T>> Elems = new List<RandomPoolEntrySaveData<T>>();
		[UsedImplicitly]
		public List<RandomPoolEntrySaveData<T>> FallbackElems = new List<RandomPoolEntrySaveData<T>>();
		public bool Fallback;
	}
}
