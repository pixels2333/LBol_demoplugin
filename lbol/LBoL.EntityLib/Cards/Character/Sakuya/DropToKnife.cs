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
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class DropToKnife : Card
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
			if (list.Count <= base.Value1)
			{
				this._allHand = list;
			}
			if (list.Count <= base.Value1)
			{
				return null;
			}
			return new SelectHandInteraction(base.Value1, base.Value1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int drop = 0;
			if (precondition != null)
			{
				IReadOnlyList<Card> selectedCards = ((SelectHandInteraction)precondition).SelectedCards;
				if (selectedCards != null)
				{
					drop = selectedCards.Count;
					yield return new DiscardManyAction(selectedCards);
				}
			}
			else if (this._allHand.Count > 0)
			{
				drop = this._allHand.Count;
				yield return new DiscardManyAction(this._allHand);
				this._allHand = null;
			}
			if (drop > 0)
			{
				yield return new AddCardsToHandAction(Library.CreateCards<Knife>(drop, false), AddCardsType.Normal);
			}
			yield break;
		}
		private List<Card> _allHand;
	}
}
