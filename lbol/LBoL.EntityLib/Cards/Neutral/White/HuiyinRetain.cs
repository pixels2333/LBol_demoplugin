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
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.ExtraTurn;
namespace LBoL.EntityLib.Cards.Neutral.White
{
	[UsedImplicitly]
	public sealed class HuiyinRetain : LimitedStopTimeCard
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(base.Battle.DiscardZone);
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(0, base.Value1, list, SelectedCardHandling.DoNothing);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> cards = ((SelectCardInteraction)precondition).SelectedCards;
				foreach (Card card in cards)
				{
					yield return new MoveCardAction(card, CardZone.Hand);
				}
				IEnumerator<Card> enumerator = null;
				foreach (Card card2 in cards)
				{
					if (card2.Zone == CardZone.Hand && !card2.IsRetain && !card2.Summoned)
					{
						card2.IsTempRetain = true;
					}
				}
				cards = null;
			}
			yield return base.BuffAction<ExtraTurn>(1, 0, 0, 0, 0.2f);
			if (base.Limited)
			{
				yield return base.DebuffAction<TimeIsLimited>(base.Battle.Player, 1, 0, 0, 0, true, 0.2f);
			}
			yield return new RequestEndPlayerTurnAction();
			yield break;
			yield break;
		}
	}
}
