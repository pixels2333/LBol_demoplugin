using System;
using LBoL.Core.Units;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	public sealed class GameRunRecordSaveData
	{
		public int TotalSeconds;
		public string SaveTimestamp;
		public string Player;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public PlayerType? PlayerType;
		public string Us;
		public GameMode Mode;
		public GameDifficulty Difficulty;
		public PuzzleFlag Puzzles;
		public ulong Seed;
		public GameResultType ResultType;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string FailingEnemyGroup;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public string FailingAdventure;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public StageRecord[] Stages = Array.Empty<StageRecord>();
		public int MaxHp;
		public int TotalMoney;
		public string BaseMana;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public CardRecordSaveData[] Cards = Array.Empty<CardRecordSaveData>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public string[] Exhibits = Array.Empty<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public string[] JadeBoxes = Array.Empty<string>();
		public int BluePoint;
		public bool IsAutoSeed;
		public bool ShowRandomResult;
		public int ReloadTimes;
		public string GameVersion;
	}
}
