using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Stations;
using LBoL.Core.Stats;
using LBoL.Core.Units;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000DD RID: 221
	public sealed class GameRunSaveData
	{
		// Token: 0x04000422 RID: 1058
		public string GameVersion;

		// Token: 0x04000423 RID: 1059
		public string GameRevision;

		// Token: 0x04000424 RID: 1060
		public SaveTiming Timing;

		// Token: 0x04000425 RID: 1061
		public string SaveTimestamp;

		// Token: 0x04000426 RID: 1062
		public int PlayedSeconds;

		// Token: 0x04000427 RID: 1063
		public GameRunStatus Status;

		// Token: 0x04000428 RID: 1064
		public bool IsNormalEndFinished;

		// Token: 0x04000429 RID: 1065
		public GameMode Mode;

		// Token: 0x0400042A RID: 1066
		public GameDifficulty Difficulty;

		// Token: 0x0400042B RID: 1067
		public PuzzleFlag Puzzles;

		// Token: 0x0400042C RID: 1068
		public ulong RootSeed;

		// Token: 0x0400042D RID: 1069
		public bool IsAutoSeed;

		// Token: 0x0400042E RID: 1070
		public ulong RootRng;

		// Token: 0x0400042F RID: 1071
		public ulong StationRng;

		// Token: 0x04000430 RID: 1072
		public ulong InitBossSeed;

		// Token: 0x04000431 RID: 1073
		public ulong ShopRng;

		// Token: 0x04000432 RID: 1074
		public ulong AdventureRng;

		// Token: 0x04000433 RID: 1075
		public ulong ExhibitRng;

		// Token: 0x04000434 RID: 1076
		public ulong ShinningExhibitRng;

		// Token: 0x04000435 RID: 1077
		public ulong CardRng;

		// Token: 0x04000436 RID: 1078
		public ulong GamerunEventRng;

		// Token: 0x04000437 RID: 1079
		public ulong BattleRng;

		// Token: 0x04000438 RID: 1080
		public ulong BattleCardRng;

		// Token: 0x04000439 RID: 1081
		public ulong ShuffleRng;

		// Token: 0x0400043A RID: 1082
		public ulong EnemyMoveRng;

		// Token: 0x0400043B RID: 1083
		public ulong EnemyBattleRng;

		// Token: 0x0400043C RID: 1084
		public ulong DebutRng;

		// Token: 0x0400043D RID: 1085
		public ulong FinalBossSeed;

		// Token: 0x0400043E RID: 1086
		public ulong UISeed;

		// Token: 0x0400043F RID: 1087
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool HasClearBonus;

		// Token: 0x04000440 RID: 1088
		public int UnlockLevel;

		// Token: 0x04000441 RID: 1089
		[UsedImplicitly]
		public List<StageSaveData> Stages = new List<StageSaveData>();

		// Token: 0x04000442 RID: 1090
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int? StageIndex;

		// Token: 0x04000443 RID: 1091
		[UsedImplicitly]
		public List<MapNodeSaveData> Path = new List<MapNodeSaveData>();

		// Token: 0x04000444 RID: 1092
		public PlayerSaveData Player;

		// Token: 0x04000445 RID: 1093
		public PlayerType PlayerType;

		// Token: 0x04000446 RID: 1094
		public string Mana;

		// Token: 0x04000447 RID: 1095
		[UsedImplicitly]
		public List<CardSaveData> Deck = new List<CardSaveData>();

		// Token: 0x04000448 RID: 1096
		public int DeckCardInstanceId;

		// Token: 0x04000449 RID: 1097
		public int Money;

		// Token: 0x0400044A RID: 1098
		public int TotalMoney;

		// Token: 0x0400044B RID: 1099
		public int UltimateUseCount;

		// Token: 0x0400044C RID: 1100
		public int ReloadTimes;

		// Token: 0x0400044D RID: 1101
		public bool ShowRandomResult;

		// Token: 0x0400044E RID: 1102
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int ShopRemoveCardCounter;

		// Token: 0x0400044F RID: 1103
		public float CardRareWeightFactor;

		// Token: 0x04000450 RID: 1104
		public List<CardWeightFactorSaveData> CardRewardWeightFactors = new List<CardWeightFactorSaveData>();

		// Token: 0x04000451 RID: 1105
		public bool CardRewardDecreaseRepeatRare;

		// Token: 0x04000452 RID: 1106
		[UsedImplicitly]
		public List<ExhibitSaveData> Exhibits = new List<ExhibitSaveData>();

		// Token: 0x04000453 RID: 1107
		[UsedImplicitly]
		public List<JadeBoxSaveData> JadeBoxes = new List<JadeBoxSaveData>();

		// Token: 0x04000454 RID: 1108
		public List<string> ShiningExhibitPool = new List<string>();

		// Token: 0x04000455 RID: 1109
		public List<string> ExhibitPool = new List<string>();

		// Token: 0x04000456 RID: 1110
		[UsedImplicitly]
		public List<string> ExhibitRecord = new List<string>();

		// Token: 0x04000457 RID: 1111
		[UsedImplicitly]
		public List<string> AdventureHistory = new List<string>();

		// Token: 0x04000458 RID: 1112
		[UsedImplicitly]
		public GameRunStats Stats = new GameRunStats();

		// Token: 0x04000459 RID: 1113
		[UsedImplicitly]
		public HashSet<string> ExtraFlags = new HashSet<string>();

		// Token: 0x0400045A RID: 1114
		[UsedImplicitly]
		public List<StageRecord> StageRecords = new List<StageRecord>();

		// Token: 0x0400045B RID: 1115
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public MapNodeSaveData EnteringNode;

		// Token: 0x0400045C RID: 1116
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public StationType? EnteredStationType;

		// Token: 0x0400045D RID: 1117
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public string BattleStationEnemyGroup;

		// Token: 0x0400045E RID: 1118
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[CanBeNull]
		public AdventureSaveData AdventureState;

		// Token: 0x0400045F RID: 1119
		public int FinalBossInitialDamage;

		// Token: 0x04000460 RID: 1120
		public string ExtraExhibitReward;
	}
}
