using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core.Stations
{
	// Token: 0x020000BD RID: 189
	public class BossStation : BattleStation
	{
		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x0600084B RID: 2123 RVA: 0x0001878A File Offset: 0x0001698A
		public override StationType Type
		{
			get
			{
				return StationType.Boss;
			}
		}

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x0600084C RID: 2124 RVA: 0x0001878E File Offset: 0x0001698E
		// (set) Token: 0x0600084D RID: 2125 RVA: 0x00018796 File Offset: 0x00016996
		public string BossId { get; internal set; }

		// Token: 0x170002A9 RID: 681
		// (get) Token: 0x0600084E RID: 2126 RVA: 0x0001879F File Offset: 0x0001699F
		// (set) Token: 0x0600084F RID: 2127 RVA: 0x000187A7 File Offset: 0x000169A7
		public Exhibit[] BossRewards { get; internal set; }

		// Token: 0x06000850 RID: 2128 RVA: 0x000187B0 File Offset: 0x000169B0
		public void GenerateBossRewards()
		{
			this.BossRewards = base.Stage.GetBossExhibits();
		}

		// Token: 0x06000851 RID: 2129 RVA: 0x000187C4 File Offset: 0x000169C4
		public override void GenerateRewards()
		{
			List<StationReward> rewards = base.Rewards;
			if (rewards != null && rewards.Count > 0)
			{
				Debug.LogError("GenerateRewards invoked while already has rewards");
			}
			float num = (float)base.GameRun.GameRunEventRng.NextInt(GlobalConfig.BossStationMoney[0], GlobalConfig.BossStationMoney[1]);
			if (base.GameRun.Puzzles.HasFlag(PuzzleFlag.LowStageRegen))
			{
				num *= 0.5f;
			}
			int num2 = base.GameRun.ModifyMoneyReward(num);
			base.AddRewards(new StationReward[]
			{
				StationReward.CreateMoney(num2),
				base.Stage.GetBossCardReward()
			});
			int level = base.Stage.Level;
			if ((level == 1 || level == 2) && Enumerable.Any<Card>(base.GameRun.BaseDeckInBossRemoveReward))
			{
				base.AddReward(StationReward.CreateRemoveCard());
			}
			base.GameRun.StationRewardGenerating.Execute(new StationEventArgs
			{
				Station = this,
				CanCancel = false
			});
		}

		// Token: 0x06000852 RID: 2130 RVA: 0x000188B7 File Offset: 0x00016AB7
		protected override EnemyGroupEntry GetEnemyGroupEntry()
		{
			return base.Stage.GetBoss();
		}

		// Token: 0x06000853 RID: 2131 RVA: 0x000188C4 File Offset: 0x00016AC4
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = StationType.Boss,
				EnemyGroup = base.EnemyGroupEntry.Id
			};
		}
	}
}
