using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class DreamCardsToHandAction : SimpleAction
	{
		internal override IEnumerable<Phase> GetPhases()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => card.IsDreamCard));
			if (list.Count > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(0, 1, list, SelectedCardHandling.DoNothing)
				{
					Description = "SelectCard.DreamCardsToHand".Localize(true)
				};
				yield return base.CreatePhase("Select", delegate
				{
					this.React(new InteractionAction(interaction, false), null, default(ActionCause?));
				}, false);
				IReadOnlyList<Card> selected = interaction.SelectedCards;
				if (selected.Count > 0)
				{
					yield return base.CreatePhase("MoveToHand", delegate
					{
						foreach (Card card3 in selected)
						{
							this.React(new MoveCardAction(card3, CardZone.Hand), null, default(ActionCause?));
						}
					}, false);
				}
				using (IEnumerator<Card> enumerator = base.Battle.EnumerateAllCards().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Card card2 = enumerator.Current;
						card2.IsDreamCard = false;
					}
					yield break;
				}
			}
			yield break;
		}
	}
}
