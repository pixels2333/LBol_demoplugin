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
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class KoishiDive : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DreamCardsAction(base.Value1, 0);
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DiscardZone, (Card card) => card.IsDreamCard));
			if (list.Count > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, base.Value2, list, SelectedCardHandling.DoNothing)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				if (interaction.SelectedCards.Count > 0)
				{
					foreach (Card card2 in interaction.SelectedCards)
					{
						yield return new MoveCardAction(card2, CardZone.Hand);
						card2.IsDreamCard = false;
						card2 = null;
					}
					IEnumerator<Card> enumerator = null;
				}
				interaction = null;
			}
			yield break;
			yield break;
		}
	}
}
