using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core.Stations
{
	public class BossStation : BattleStation
	{
		public override StationType Type
		{
			get
			{
				return StationType.Boss;
			}
		}
		public string BossId { get; internal set; }
		public Exhibit[] BossRewards { get; internal set; }
		public void GenerateBossRewards()
		{
			this.BossRewards = base.Stage.GetBossExhibits();
		}
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
		protected override EnemyGroupEntry GetEnemyGroupEntry()
		{
			return base.Stage.GetBoss();
		}
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
