using System;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using Yarn;
namespace LBoL.EntityLib.Adventures.Shared23
{
	[AdventureInfo(WeighterType = typeof(HinaCollect.HinaCollectWeighter))]
	public sealed class HinaCollect : Adventure
	{
		protected override void InitVariables(IVariableStorage storage)
		{
			Card randomCurseCard = base.GameRun.GetRandomCurseCard(base.GameRun.AdventureRng, false);
			storage.SetValue("$card", randomCurseCard.Id);
		}
		[RuntimeCommand("yes", "")]
		[UsedImplicitly]
		public void Yes()
		{
			base.GameRun.RemoveDeckCards(Enumerable.Where<Card>(base.GameRun.BaseDeckWithoutUnremovable, (Card c) => c.CardType == CardType.Misfortune), true);
		}
		private class HinaCollectWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)(Enumerable.Any<Card>(gameRun.BaseDeckWithoutUnremovable, (Card c) => c.CardType == CardType.Misfortune) ? 1 : 0);
			}
		}
	}
}
