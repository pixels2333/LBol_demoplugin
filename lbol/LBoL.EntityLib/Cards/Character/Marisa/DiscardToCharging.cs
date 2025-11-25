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
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class DiscardToCharging : Card
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
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, list.Count, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
			IReadOnlyList<Card> cards = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards : null);
			if (cards != null)
			{
				yield return new DiscardManyAction(cards);
				yield return base.BuffAction<Charging>(base.Value1 * cards.Count, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
