using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.White
{
	[UsedImplicitly]
	public sealed class YunyouShengdi : Card
	{
		public override Interaction Precondition()
		{
			if (this.IsUpgraded)
			{
				return null;
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this && hand.CanUpgradeAndPositive));
			if (list.Count == 1)
			{
				this.oneTargetHand = list[0];
			}
			if (list.Count <= 1)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (this.IsUpgraded)
			{
				yield return base.UpgradeAllHandsAction();
			}
			else if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new UpgradeCardAction(card);
				}
			}
			else if (this.oneTargetHand != null)
			{
				yield return new UpgradeCardAction(this.oneTargetHand);
				this.oneTargetHand = null;
			}
			yield break;
		}
		private Card oneTargetHand;
	}
}
