using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200015D RID: 349
	public class AddCardsToExileAction : SimpleEventBattleAction<CardsEventArgs>
	{
		// Token: 0x170004D4 RID: 1236
		// (get) Token: 0x06000DC0 RID: 3520 RVA: 0x00025CDF File Offset: 0x00023EDF
		public AddCardsType PresentationType { get; }

		// Token: 0x06000DC1 RID: 3521 RVA: 0x00025CE7 File Offset: 0x00023EE7
		public AddCardsToExileAction(params Card[] cards)
			: this(cards, AddCardsType.Normal)
		{
		}

		// Token: 0x06000DC2 RID: 3522 RVA: 0x00025CF1 File Offset: 0x00023EF1
		public AddCardsToExileAction(IEnumerable<Card> cards, AddCardsType presentationType = AddCardsType.Normal)
		{
			base.Args = new CardsEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards)
			};
			this.PresentationType = presentationType;
		}

		// Token: 0x06000DC3 RID: 3523 RVA: 0x00025D17 File Offset: 0x00023F17
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.CardsAddingToExile);
		}

		// Token: 0x06000DC4 RID: 3524 RVA: 0x00025D2C File Offset: 0x00023F2C
		protected override void MainPhase()
		{
			List<Card> list = new List<Card>();
			foreach (Card card in base.Args.Cards)
			{
				if (base.Battle.AddCardToExile(card) == CancelCause.None)
				{
					list.Add(card);
				}
			}
			if (!Enumerable.SequenceEqual<Card>(list, base.Args.Cards))
			{
				base.Args.Cards = list.ToArray();
				base.Args.IsModified = true;
			}
		}

		// Token: 0x06000DC5 RID: 3525 RVA: 0x00025DA2 File Offset: 0x00023FA2
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.CardsAddedToExile);
		}
	}
}
