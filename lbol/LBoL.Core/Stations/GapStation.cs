using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Core.GapOptions;
using LBoL.Core.SaveData;
using UnityEngine;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C1 RID: 193
	public sealed class GapStation : Station
	{
		// Token: 0x170002AE RID: 686
		// (get) Token: 0x06000861 RID: 2145 RVA: 0x0001899A File Offset: 0x00016B9A
		public override StationType Type
		{
			get
			{
				return StationType.Gap;
			}
		}

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x06000862 RID: 2146 RVA: 0x0001899D File Offset: 0x00016B9D
		public List<GapOption> GapOptions { get; } = new List<GapOption>();

		// Token: 0x06000863 RID: 2147 RVA: 0x000189A8 File Offset: 0x00016BA8
		protected internal override void OnEnter()
		{
			base.Status = StationStatus.Rest;
			DrinkTea drinkTea = Library.CreateGapOption<DrinkTea>();
			drinkTea.Rate = base.GameRun.DrinkTeaHealRate;
			drinkTea.Value = base.GameRun.DrinkTeaHealValue;
			drinkTea.AdditionalHeal = base.GameRun.DrinkTeaAdditionalHeal;
			drinkTea.AdditionalPower = base.GameRun.DrinkTeaAdditionalEnergy;
			drinkTea.AdditionalCardReward = base.GameRun.DrinkTeaCardRewardFlag;
			this.GapOptions.Add(drinkTea);
			UpgradeCard upgradeCard = Library.CreateGapOption<UpgradeCard>();
			if (base.GameRun.Puzzles.HasFlag(PuzzleFlag.PayForUpgrade))
			{
				upgradeCard.Price = 25;
			}
			this.GapOptions.Add(upgradeCard);
			base.GameRun.GapOptionsGenerating.Execute(new StationEventArgs
			{
				Station = this,
				CanCancel = false
			});
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x00018A80 File Offset: 0x00016C80
		public void DrinkTea(DrinkTea drinkTea)
		{
			base.GameRun.Heal(drinkTea.Value + drinkTea.AdditionalHeal, true, null);
			if (drinkTea.AdditionalPower > 0)
			{
				base.GameRun.GainPower(drinkTea.AdditionalPower, false);
			}
			if (drinkTea.AdditionalCardReward > 0)
			{
				List<StationReward> rewards = base.Rewards;
				if (rewards != null && rewards.Count > 0)
				{
					Debug.LogError("DrinkTea invoked while already has rewards");
				}
				base.AddReward(base.Stage.GetDrinkTeaCardReward(drinkTea));
			}
		}

		// Token: 0x06000865 RID: 2149 RVA: 0x00018AFA File Offset: 0x00016CFA
		public IEnumerator FindExhibitRunner()
		{
			int num = this.GapOptions.FindIndex((GapOption o) => o is FindExhibit);
			if (num >= 0)
			{
				yield return base.GameRun.GainExhibitRunner(base.GameRun.CurrentStage.GetEliteEnemyExhibit(), true, new VisualSourceData
				{
					SourceType = VisualSourceType.Gap,
					Index = num
				});
			}
			else
			{
				Debug.LogError("[GapStation] Cannot FindExhibit without a FindExhibit gap option");
			}
			yield break;
		}

		// Token: 0x06000866 RID: 2150 RVA: 0x00018B09 File Offset: 0x00016D09
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type
			};
		}
	}
}
