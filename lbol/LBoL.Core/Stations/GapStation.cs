using System;
using System.Collections;
using System.Collections.Generic;
using LBoL.Core.GapOptions;
using LBoL.Core.SaveData;
using UnityEngine;
namespace LBoL.Core.Stations
{
	public sealed class GapStation : Station
	{
		public override StationType Type
		{
			get
			{
				return StationType.Gap;
			}
		}
		public List<GapOption> GapOptions { get; } = new List<GapOption>();
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
		internal override StationRecord GenerateRecord()
		{
			return new StationRecord
			{
				Type = this.Type
			};
		}
	}
}
