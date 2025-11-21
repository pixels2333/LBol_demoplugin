using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200015B RID: 347
	public class AddCardsToDiscardAction : SimpleEventBattleAction<CardsEventArgs>
	{
		// Token: 0x170004D2 RID: 1234
		// (get) Token: 0x06000DB5 RID: 3509 RVA: 0x00025B05 File Offset: 0x00023D05
		public AddCardsType PresentationType { get; }

		// Token: 0x06000DB6 RID: 3510 RVA: 0x00025B0D File Offset: 0x00023D0D
		public AddCardsToDiscardAction(params Card[] cards)
			: this(cards, AddCardsType.Normal)
		{
		}

		// Token: 0x06000DB7 RID: 3511 RVA: 0x00025B17 File Offset: 0x00023D17
		public AddCardsToDiscardAction(IEnumerable<Card> cards, AddCardsType presentationType = AddCardsType.Normal)
		{
			base.Args = new CardsEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards)
			};
			this.PresentationType = presentationType;
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x00025B3D File Offset: 0x00023D3D
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.CardsAddingToDiscard);
		}

		// Token: 0x06000DB9 RID: 3513 RVA: 0x00025B50 File Offset: 0x00023D50
		protected override void MainPhase()
		{
			List<Card> list = new List<Card>();
			foreach (Card card in base.Args.Cards)
			{
				if (base.Battle.AddCardToDiscard(card) == CancelCause.None)
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

		// Token: 0x06000DBA RID: 3514 RVA: 0x00025BC6 File Offset: 0x00023DC6
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.CardsAddedToDiscard);
		}
	}
}
