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
namespace LBoL.EntityLib.Cards.Character.Cirno
{
	[UsedImplicitly]
	public sealed class CirnoEcho : Card
	{
		public override Interaction Precondition()
		{
			IReadOnlyList<Card> discardZone = base.Battle.DiscardZone;
			if (discardZone.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(base.Value1, base.Value1, discardZone, SelectedCardHandling.DoNothing);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			Card card = ((selectCardInteraction != null) ? Enumerable.FirstOrDefault<Card>(selectCardInteraction.SelectedCards) : null);
			if (card != null)
			{
				if (card.CanBeDuplicated)
				{
					card.IsEcho = true;
				}
				yield return new MoveCardAction(card, CardZone.Hand);
			}
			yield break;
		}
	}
}
