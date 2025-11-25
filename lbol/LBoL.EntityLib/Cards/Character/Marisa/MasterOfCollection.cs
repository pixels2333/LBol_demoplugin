using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Marisa;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class MasterOfCollection : Card
	{
		public override Interaction Precondition()
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this && !hand.IsRetain));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(0, base.Value2, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectHandInteraction)precondition).SelectedCards;
				if (selectedCards.NotEmpty<Card>())
				{
					foreach (Card card in selectedCards)
					{
						card.IsRetain = true;
					}
				}
			}
			yield return base.BuffAction<MasterOfCollectionSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
