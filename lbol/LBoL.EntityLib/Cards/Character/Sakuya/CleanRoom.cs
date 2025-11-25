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
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class CleanRoom : Card
	{
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this));
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
			Card discard = null;
			if (precondition != null)
			{
				Card card = ((SelectHandInteraction)precondition).SelectedCards[0];
				if (card != null)
				{
					yield return new DiscardAction(card);
					discard = card;
				}
				card = null;
			}
			else if (this.oneTargetHand != null)
			{
				yield return new DiscardAction(this.oneTargetHand);
				discard = this.oneTargetHand;
				this.oneTargetHand = null;
			}
			yield return new DrawManyCardAction(base.Value1);
			if (discard is Knife)
			{
				yield return base.UpgradeAllHandsAction();
			}
			else
			{
				yield return base.UpgradeRandomHandAction(base.Value2, CardType.Unknown);
			}
			yield break;
		}
		private Card oneTargetHand;
	}
}
