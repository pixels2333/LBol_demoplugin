using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using YamlDotNet.Serialization;

namespace LBoL.Core.Stats
{
	// Token: 0x020000B8 RID: 184
	[Serializable]
	public sealed class GameRunStats
	{
		// Token: 0x0600082E RID: 2094 RVA: 0x00018390 File Offset: 0x00016590
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

		// Token: 0x0400037A RID: 890
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> ExhibitsRevealed = new List<string>();

		// Token: 0x0400037B RID: 891
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> CardsRevealed = new List<string>();

		// Token: 0x0400037C RID: 892
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<string> Bosses = new List<string>();

		// Token: 0x0400037D RID: 893
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitEmptyCollections)]
		public List<StageStats> Stages = new List<StageStats>();

		// Token: 0x0400037E RID: 894
		public bool PlayerSuicide;

		// Token: 0x0400037F RID: 895
		public int ContinuousTurnCount;

		// Token: 0x04000380 RID: 896
		public int PerfectElite;

		// Token: 0x04000381 RID: 897
		public int PerfectBoss;

		// Token: 0x04000382 RID: 898
		public int MaxSingleAttackDamage;

		// Token: 0x04000383 RID: 899
		public int ShopConsumed;

		// Token: 0x04000384 RID: 900
		public int MaxHpGained;

		// Token: 0x04000385 RID: 901
		public int TotalGainTreasure;

		// Token: 0x04000386 RID: 902
		[YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitDefaults)]
		[DefaultValue(true)]
		public bool NoExhibitFlag = true;
	}
}
