using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using YamlDotNet.Serialization;

namespace LBoL.Core.SaveData
{
	// Token: 0x020000E3 RID: 227
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public sealed class ProfileSaveData
	{
		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x060008E6 RID: 2278 RVA: 0x00019FE0 File Offset: 0x000181E0
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

		// Token: 0x060008E7 RID: 2279 RVA: 0x0001A030 File Offset: 0x00018230
		public GameDifficulty? GetHighestDifficulty(string characterId)
		{
			CharacterStatsSaveData characterStatsSaveData = Enumerable.FirstOrDefault<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.CharacterId == characterId);
			if (characterStatsSaveData == null)
			{
				return default(GameDifficulty?);
			}
			return characterStatsSaveData.HighestSuccessDifficulty;
		}

		// Token: 0x060008E8 RID: 2280 RVA: 0x0001A074 File Offset: 0x00018274
		public GameDifficulty? GetHighestPerfectDifficulty(string characterId)
		{
			CharacterStatsSaveData characterStatsSaveData = Enumerable.FirstOrDefault<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.CharacterId == characterId);
			if (characterStatsSaveData == null)
			{
				return default(GameDifficulty?);
			}
			return characterStatsSaveData.HighestPerfectSuccessDifficulty;
		}

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x060008E9 RID: 2281 RVA: 0x0001A0B8 File Offset: 0x000182B8
		[YamlIgnore]
		public int HighestBluePoint
		{
			get
			{
				return Enumerable.Max<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.HighestBluePoint);
			}
		}

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x060008EA RID: 2282 RVA: 0x0001A0E4 File Offset: 0x000182E4
		[YamlIgnore]
		public int TotalBluePoint
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.TotalBluePoint) + this.BluePoint;
			}
		}

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x060008EB RID: 2283 RVA: 0x0001A117 File Offset: 0x00018317
		[YamlIgnore]
		public int TotalPlaySeconds
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.TotalPlaySeconds);
			}
		}

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x060008EC RID: 2284 RVA: 0x0001A143 File Offset: 0x00018343
		[YamlIgnore]
		public int PerfectSuccessCount
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.PerfectSuccessCount);
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x060008ED RID: 2285 RVA: 0x0001A16F File Offset: 0x0001836F
		[YamlIgnore]
		public int SuccessCount
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.SuccessCount);
			}
		}

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x060008EE RID: 2286 RVA: 0x0001A19B File Offset: 0x0001839B
		[YamlIgnore]
		public int FailCount
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.FailCount);
			}
		}

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x060008EF RID: 2287 RVA: 0x0001A1C7 File Offset: 0x000183C7
		[YamlIgnore]
		public int TotalPuzzleCount
		{
			get
			{
				return Enumerable.Sum<CharacterStatsSaveData>(this.CharacterStats, (CharacterStatsSaveData c) => c.PuzzleCount);
			}
		}

		// Token: 0x04000482 RID: 1154
		public string Name;

		// Token: 0x04000483 RID: 1155
		public string CreationTimestamp;

		// Token: 0x04000484 RID: 1156
		public string SaveTimestamp;

		// Token: 0x04000485 RID: 1157
		public string GameVersion;

		// Token: 0x04000486 RID: 1158
		public string GameRevision;

		// Token: 0x04000487 RID: 1159
		public int Exp;

		// Token: 0x04000488 RID: 1160
		public int BluePoint;

		// Token: 0x04000489 RID: 1161
		public bool OpeningPlayed;

		// Token: 0x0400048A RID: 1162
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public bool HasClearBonus;

		// Token: 0x0400048B RID: 1163
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> CardsRevealed = new List<string>();

		// Token: 0x0400048C RID: 1164
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> ExhibitsRevealed = new List<string>();

		// Token: 0x0400048D RID: 1165
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> EnemyGroupRevealed = new List<string>();

		// Token: 0x0400048E RID: 1166
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public CharacterStatsSaveData[] CharacterStats = Array.Empty<CharacterStatsSaveData>();

		// Token: 0x0400048F RID: 1167
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public GameSettingsSaveData Settings = new GameSettingsSaveData();

		// Token: 0x04000490 RID: 1168
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
		public HintStatusSaveData HintStatus = new HintStatusSaveData();

		// Token: 0x04000491 RID: 1169
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> Achievements = new List<string>();

		// Token: 0x04000492 RID: 1170
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		public int PayMoneyCount;
	}
}
