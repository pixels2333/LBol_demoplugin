using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using Yarn;
namespace LBoL.EntityLib.Adventures.Shared23
{
	[AdventureInfo(WeighterType = typeof(NazrinDetect.NazrinDetectWeighter))]
	public sealed class NazrinDetect : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			storage.SetValue("$hpReward", (float)base.GameRun.Player.MaxHp);
		}
		[RuntimeCommand("cardReward", "")]
		[UsedImplicitly]
		public void RewardCard()
		{
			Card randomCurseCard = base.GameRun.GetRandomCurseCard(base.GameRun.AdventureRng, false);
			base.Storage.SetValue("$cardReward", randomCurseCard.ToString());
		}
		[RuntimeCommand("exhibitReward", "")]
		[UsedImplicitly]
		private void RewardExhibit()
		{
			Exhibit eliteEnemyExhibit = base.Stage.GetEliteEnemyExhibit();
			base.Storage.SetValue("$exhibitReward", eliteEnemyExhibit.Id);
			base.Storage.SetValue("$isSentinel", eliteEnemyExhibit.Config.IsSentinel);
		}
		private class NazrinDetectWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((gameRun.Money >= 50) ? 1 : 0);
			}
		}
	}
}
