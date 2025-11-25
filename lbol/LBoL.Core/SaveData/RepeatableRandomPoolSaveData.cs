using System;
using System.Collections.Generic;
using JetBrains.Annotations;
namespace LBoL.Core.SaveData
{
	public sealed class RepeatableRandomPoolSaveData<T>
	{
		[UsedImplicitly]
		public List<RandomPoolEntrySaveData<T>> Elems = new List<RandomPoolEntrySaveData<T>>();
	}
}
