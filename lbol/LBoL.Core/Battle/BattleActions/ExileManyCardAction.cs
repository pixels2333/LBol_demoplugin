using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public class ExileManyCardAction : SimpleAction
	{
		public Card[] Cards
		{
			get
			{
				return this._cards;
			}
		}
		public ExileManyCardAction(IEnumerable<Card> cards)
		{
			this._cards = Enumerable.ToArray<Card>(cards);
		}
		protected override void ResolvePhase()
		{
			base.React(new Reactor(Enumerable.Select<Card, ExileCardAction>(this._cards, (Card c) => new ExileCardAction(c, this))), null, default(ActionCause?));
		}
		private readonly Card[] _cards;
	}
}
