using System;
namespace LBoL.Core.SaveData
{
	public sealed class AdventureSaveData
	{
		public string AdventureId;
		public string NodeName;
		public string StorageYaml;
		public AdvSlotSaveData[] Slots;
	}
}
