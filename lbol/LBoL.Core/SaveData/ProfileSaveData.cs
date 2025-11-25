using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using YamlDotNet.Serialization;
namespace LBoL.Core.SaveData
{
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public sealed class ProfileSaveData
	{
		[YamlIgnore]
		public GameDifficulty? HighestSuccessDifficulty
		{
			get
			{
				if (!this.CharacterStats.Empty<CharacterStatsSaveData>())
				{
					return Enumerable.Max<CharacterStatsSaveData, GameDifficulty?>(this.CharacterStats, (CharacterStatsSaveData c) => c.HighestSuccessDifficulty);
				}
				return default(GameDifficulty?);
			}
		}
		public GameDifficulty? GetHighestDifficulty(string characterId)
		{
			CharacterStatsSaveData characterStatsSaveData = Enumerable.FirstOrDefault<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.CharacterId == characterId);
			if (characterStatsSaveData == null)
			{
				return default(GameDifficulty?);
			}
			return characterStatsSaveData.HighestSuccessDifficulty;
		}
		public GameDifficulty? GetHighestPerfectDifficulty(string characterId)
		{
			CharacterStatsSaveData characterStatsSaveData = Enumerable.FirstOrDefault<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.CharacterId == characterId);
			if (characterStatsSaveData == null)
			{
				return default(GameDifficulty?);
			}
			return characterStatsSaveData.HighestPerfectSuccessDifficulty;
		}
		[YamlIgnore]
		public int HighestBluePoint
		{
			get
			{
				return Enumerable.Max<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.HighestBluePoint);
			}
		}
		[YamlIgnore]
		public int TotalBluePoint
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.TotalBluePoint) + this.BluePoint;
			}
		}
		[YamlIgnore]
		public int TotalPlaySeconds
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.TotalPlaySeconds);
			}
		}
		[YamlIgnore]
		public int PerfectSuccessCount
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.PerfectSuccessCount);
			}
		}
		[YamlIgnore]
		public int SuccessCount
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.SuccessCount);
			}
		}
		[YamlIgnore]
		public int FailCount
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.FailCount);
			}
		}
		[YamlIgnore]
		public int TotalPuzzleCount
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.PuzzleCount);
			}
		}
		public string Name;
		public string CreationTimestamp;
		public string SaveTimestamp;
		public string GameVersion;
		public string GameRevision;
		public int Exp;
		public int BluePoint;
		public bool OpeningPlayed;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool HasClearBonus;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> CardsRevealed = new List<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> ExhibitsRevealed = new List<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> EnemyGroupRevealed = new List<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public CharacterStatsSaveData[] CharacterStats = Array.Empty<CharacterStatsSaveData>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public GameSettingsSaveData Settings = new GameSettingsSaveData();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public HintStatusSaveData HintStatus = new HintStatusSaveData();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> Achievements = new List<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int PayMoneyCount;
	}
}
