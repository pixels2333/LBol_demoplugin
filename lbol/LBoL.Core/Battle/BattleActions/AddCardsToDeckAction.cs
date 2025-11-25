using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public class AddCardsToDeckAction : SimpleEventBattleAction<CardsEventArgs>
	{
		public AddCardsToDeckAction(params Card[] cards)
			: this(cards)
		{
		}
		public AddCardsToDeckAction(IEnumerable<Card> cards)
		{
			base.Args = new CardsEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards)
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.GameRun.DeckCardsAdding);
		}
		protected override void MainPhase()
		{
			Card[] array = base.Battle.GameRun.InternalAddDeckCards(base.Args.Cards);
			if (!Enumerable.SequenceEqual<Card>(array, base.Args.Cards))
			{
				base.Args.Cards = Enumerable.ToArray<Card>(array);
				base.Args.IsModified = true;
			}
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.GameRun.DeckCardsAdded);
		}
	}
}
