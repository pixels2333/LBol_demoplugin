using System;
using LBoL.Core.Units;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000D2 RID: 210
	public sealed class GameRunRecordSaveData
	{
		// Token: 0x040003E2 RID: 994
		public int TotalSeconds;

		// Token: 0x040003E3 RID: 995
		public string SaveTimestamp;

		// Token: 0x040003E4 RID: 996
		public string Player;

		// Token: 0x040003E5 RID: 997
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public PlayerType? PlayerType;

		// Token: 0x040003E6 RID: 998
		public string Us;

		// Token: 0x040003E7 RID: 999
		public GameMode Mode;

		// Token: 0x040003E8 RID: 1000
		public GameDifficulty Difficulty;

		// Token: 0x040003E9 RID: 1001
		public PuzzleFlag Puzzles;

		// Token: 0x040003EA RID: 1002
		public ulong Seed;

		// Token: 0x040003EB RID: 1003
		public GameResultType ResultType;

		// Token: 0x040003EC RID: 1004
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string FailingEnemyGroup;

		// Token: 0x040003ED RID: 1005
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string FailingAdventure;

		// Token: 0x040003EE RID: 1006
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public StageRecord[] Stages = Array.Empty<StageRecord>();

		// Token: 0x040003EF RID: 1007
		public int MaxHp;

		// Token: 0x040003F0 RID: 1008
		public int TotalMoney;

		// Token: 0x040003F1 RID: 1009
		public string BaseMana;

		// Token: 0x040003F2 RID: 1010
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public CardRecordSaveData[] Cards = Array.Empty<CardRecordSaveData>();

		// Token: 0x040003F3 RID: 1011
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public string[] Exhibits = Array.Empty<string>();

		// Token: 0x040003F4 RID: 1012
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public string[] JadeBoxes = Array.Empty<string>();

		// Token: 0x040003F5 RID: 1013
		public int BluePoint;

		// Token: 0x040003F6 RID: 1014
		public bool IsAutoSeed;

		// Token: 0x040003F7 RID: 1015
		public bool ShowRandomResult;

		// Token: 0x040003F8 RID: 1016
		public int ReloadTimes;

		// Token: 0x040003F9 RID: 1017
		public string GameVersion;
	}
}
