using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public class DiscardManyAction : SimpleAction
	{
		public DiscardManyAction(IEnumerable<Card> cards)
		{
			this._cards = Enumerable.ToArray<Card>(cards);
		}
		protected override void ResolvePhase()
		{
			base.React(new Reactor(Enumerable.Select<Card, DiscardAction>(this._cards, (Card c) => new DiscardAction(c))), null, default(ActionCause?));
		}
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
		private readonly Card[] _cards;
	}
}
