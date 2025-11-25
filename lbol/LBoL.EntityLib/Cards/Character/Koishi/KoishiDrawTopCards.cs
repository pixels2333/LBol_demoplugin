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
	public sealed class KoishiDrawTopCards : Card
	{
		[UsedImplicitly]
		public int DrawCount
		{
			get
			{
				return Math.Max(0, base.Value1 - 1);
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Battle.DrawZone.Count > 0)
			{
				List<Card> cards = Enumerable.ToList<Card>(Enumerable.Take<Card>(base.Battle.DrawZone, base.Value1));
				if (cards.Count > 0)
				{
					MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(cards, false, false, false)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					Card selectedCard = interaction.SelectedCard;
					cards.Remove(selectedCard);
					if (cards.Count > 0)
					{
						foreach (Card card in cards)
						{
							if (card.Zone == CardZone.Draw)
							{
								yield return new MoveCardAction(card, CardZone.Hand);
							}
						}
						List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
					}
					interaction = null;
				}
				cards = null;
			}
			yield break;
			yield break;
		}
	}
}
