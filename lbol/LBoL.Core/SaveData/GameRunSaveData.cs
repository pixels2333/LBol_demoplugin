using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Stations;
using LBoL.Core.Stats;
using LBoL.Core.Units;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class GameRunSaveData
	{
		public string GameVersion;
		public string GameRevision;
		public SaveTiming Timing;
		public string SaveTimestamp;
		public int PlayedSeconds;
		public GameRunStatus Status;
		public bool IsNormalEndFinished;
		public GameMode Mode;
		public GameDifficulty Difficulty;
		public PuzzleFlag Puzzles;
		public ulong RootSeed;
		public bool IsAutoSeed;
		public ulong RootRng;
		public ulong StationRng;
		public ulong InitBossSeed;
		public ulong ShopRng;
		public ulong AdventureRng;
		public ulong ExhibitRng;
		public ulong ShinningExhibitRng;
		public ulong CardRng;
		public ulong GamerunEventRng;
		public ulong BattleRng;
		public ulong BattleCardRng;
		public ulong ShuffleRng;
		public ulong EnemyMoveRng;
		public ulong EnemyBattleRng;
		public ulong DebutRng;
		public ulong FinalBossSeed;
		public ulong UISeed;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool HasClearBonus;
		public int UnlockLevel;
		[UsedImplicitly]
		public List<StageSaveData> Stages = new List<StageSaveData>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? StageIndex;
		[UsedImplicitly]
		public List<MapNodeSaveData> Path = new List<MapNodeSaveData>();
		public PlayerSaveData Player;
		public PlayerType PlayerType;
		public string Mana;
		[UsedImplicitly]
		public List<CardSaveData> Deck = new List<CardSaveData>();
		public int DeckCardInstanceId;
		public int Money;
		public int TotalMoney;
		public int UltimateUseCount;
		public int ReloadTimes;
		public bool ShowRandomResult;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int ShopRemoveCardCounter;
		public float CardRareWeightFactor;
		public List<CardWeightFactorSaveData> CardRewardWeightFactors = new List<CardWeightFactorSaveData>();
		public bool CardRewardDecreaseRepeatRare;
		[UsedImplicitly]
		public List<ExhibitSaveData> Exhibits = new List<ExhibitSaveData>();
		[UsedImplicitly]
		public List<JadeBoxSaveData> JadeBoxes = new List<JadeBoxSaveData>();
		public List<string> ShiningExhibitPool = new List<string>();
		public List<string> ExhibitPool = new List<string>();
		[UsedImplicitly]
		public List<string> ExhibitRecord = new List<string>();
		[UsedImplicitly]
		public List<string> AdventureHistory = new List<string>();
		[UsedImplicitly]
		public GameRunStats Stats = new GameRunStats();
		[UsedImplicitly]
		public HashSet<string> ExtraFlags = new HashSet<string>();
		[UsedImplicitly]
		public List<StageRecord> StageRecords = new List<StageRecord>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public MapNodeSaveData EnteringNode;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public StationType? EnteredStationType;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public string BattleStationEnemyGroup;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public AdventureSaveData AdventureState;
		public int FinalBossInitialDamage;
		public string ExtraExhibitReward;
	}
}
