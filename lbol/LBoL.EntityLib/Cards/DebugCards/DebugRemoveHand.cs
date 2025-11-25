using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.DebugCards
{
	[UsedImplicitly]
	public sealed class DebugRemoveHand : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
			if (!list.Empty<Card>())
			{
				return new SelectHandInteraction(0, list.Count, list);
			}
			return null;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			if (selectHandInteraction == null)
			{
				yield break;
			}
			IReadOnlyList<Card> selectedCards = selectHandInteraction.SelectedCards;
			foreach (Card card in selectedCards)
			{
				if (card.Battle != null)
				{
					yield return new RemoveCardAction(card);
				}
			}
			IEnumerator<Card> enumerator = null;
			yield break;
			yield break;
		}
	}
}
