using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class StageSaveData
	{
		public string Name;
		public int Index;
		public ulong MapSeed;
		public int Level;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public string SelectedBoss;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public string DebutAdventure;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool IsNormalFinalStage;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool IsTrueEndFinalStage;
		public UniqueRandomPoolSaveData<string> AdventurePool;
		public UniqueRandomPoolSaveData<string> EnemyPoolAct1;
		public UniqueRandomPoolSaveData<string> EnemyPoolAct2;
		public UniqueRandomPoolSaveData<string> EnemyPoolAct3;
		public UniqueRandomPoolSaveData<string> EliteEnemyPool;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> AdventureHistory = new List<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> ExtraFlags = new List<string>();
	}
}
