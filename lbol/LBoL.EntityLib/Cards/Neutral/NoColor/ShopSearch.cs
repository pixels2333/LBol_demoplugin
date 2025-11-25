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
namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	[UsedImplicitly]
	public sealed class ShopSearch : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZoneToShow, delegate(Card card)
			{
				CardType cardType = card.CardType;
				return cardType == CardType.Attack || cardType == CardType.Defense;
			}));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(base.Value1, base.Value1, list, SelectedCardHandling.DoNothing);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			IReadOnlyList<Card> readOnlyList = ((selectCardInteraction != null) ? selectCardInteraction.SelectedCards : null);
			if (readOnlyList != null && readOnlyList.Count > 0)
			{
				foreach (Card card in readOnlyList)
				{
					yield return new MoveCardAction(card, CardZone.Hand);
				}
				IEnumerator<Card> enumerator = null;
			}
			yield break;
			yield break;
		}
	}
}
