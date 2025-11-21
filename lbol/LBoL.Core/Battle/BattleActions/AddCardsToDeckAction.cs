using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200015A RID: 346
	public class AddCardsToDeckAction : SimpleEventBattleAction<CardsEventArgs>
	{
		// Token: 0x06000DB0 RID: 3504 RVA: 0x00025A52 File Offset: 0x00023C52
		public AddCardsToDeckAction(params Card[] cards)
			: this(cards)
		{
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x00025A5B File Offset: 0x00023C5B
		public AddCardsToDeckAction(IEnumerable<Card> cards)
		{
			base.Args = new CardsEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards)
			};
		}

		// Token: 0x06000DB2 RID: 3506 RVA: 0x00025A7A File Offset: 0x00023C7A
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.GameRun.DeckCardsAdding);
		}

		// Token: 0x06000DB3 RID: 3507 RVA: 0x00025A94 File Offset: 0x00023C94
		protected override void MainPhase()
		{
			Card[] array = base.Battle.GameRun.InternalAddDeckCards(base.Args.Cards);
			if (!Enumerable.SequenceEqual<Card>(array, base.Args.Cards))
			{
				base.Args.Cards = Enumerable.ToArray<Card>(array);
				base.Args.IsModified = true;
			}
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x00025AED File Offset: 0x00023CED
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.GameRun.DeckCardsAdded);
		}
	}
}
