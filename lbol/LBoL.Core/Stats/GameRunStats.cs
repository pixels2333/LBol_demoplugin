using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;
namespace LBoL.Core.Stats
{
	[Serializable]
	public sealed class GameRunStats
	{
		public GameRunStats Clone()
		{
			return new GameRunStats
			{
				ExhibitsRevealed = Enumerable.ToList<string>(this.ExhibitsRevealed),
				CardsRevealed = Enumerable.ToList<string>(this.ExhibitsRevealed),
				Bosses = Enumerable.ToList<string>(this.Bosses),
				Stages = Enumerable.ToList<StageStats>(this.Stages),
				PlayerSuicide = this.PlayerSuicide,
				ContinuousTurnCount = this.ContinuousTurnCount,
				PerfectElite = this.PerfectElite,
				PerfectBoss = this.PerfectBoss,
				MaxSingleAttackDamage = this.MaxSingleAttackDamage,
				ShopConsumed = this.ShopConsumed,
				MaxHpGained = this.MaxHpGained,
				TotalGainTreasure = this.TotalGainTreasure,
				NoExhibitFlag = this.NoExhibitFlag
			};
		}
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> ExhibitsRevealed = new List<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> CardsRevealed = new List<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> Bosses = new List<string>();
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<StageStats> Stages = new List<StageStats>();
		public bool PlayerSuicide;
		public int ContinuousTurnCount;
		public int PerfectElite;
		public int PerfectBoss;
		public int MaxSingleAttackDamage;
		public int ShopConsumed;
		public int MaxHpGained;
		public int TotalGainTreasure;
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[DefaultValue(true)]
		public bool NoExhibitFlag = true;
	}
}
