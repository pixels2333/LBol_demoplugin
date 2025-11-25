using System;
using System.Collections.Generic;
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
	public sealed class SakuyaFuneral : Card
	{
		public override Interaction Precondition()
		{
			if (base.Battle.ExileZone.Count <= 0)
			{
				return null;
			}
			return new SelectCardInteraction(1, 1, base.Battle.ExileZone, SelectedCardHandling.DoNothing);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.SacrificeAction(base.Value1);
			SelectCardInteraction selectCardInteraction = (SelectCardInteraction)precondition;
			Card card = ((selectCardInteraction != null) ? selectCardInteraction.SelectedCards[0] : null);
			if (card != null)
			{
				yield return new MoveCardAction(card, CardZone.Hand);
				card.SetTurnCost(base.Mana);
			}
			yield break;
		}
	}
}
