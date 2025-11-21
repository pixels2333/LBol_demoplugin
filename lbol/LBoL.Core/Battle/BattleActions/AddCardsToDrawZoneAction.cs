using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200015C RID: 348
	public class AddCardsToDrawZoneAction : SimpleEventBattleAction<CardsAddingToDrawZoneEventArgs>
	{
		// Token: 0x170004D3 RID: 1235
		// (get) Token: 0x06000DBB RID: 3515 RVA: 0x00025BD9 File Offset: 0x00023DD9
		public AddCardsType PresentationType { get; }

		// Token: 0x06000DBC RID: 3516 RVA: 0x00025BE1 File Offset: 0x00023DE1
		public AddCardsToDrawZoneAction(IEnumerable<Card> cards, DrawZoneTarget target, AddCardsType presentationType = AddCardsType.Normal)
		{
			base.Args = new CardsAddingToDrawZoneEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards),
				DrawZoneTarget = target
			};
			this.PresentationType = presentationType;
		}

		// Token: 0x06000DBD RID: 3517 RVA: 0x00025C0E File Offset: 0x00023E0E
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.CardsAddingToDrawZone);
		}

		// Token: 0x06000DBE RID: 3518 RVA: 0x00025C24 File Offset: 0x00023E24
		protected override void MainPhase()
		{
			List<Card> list = new List<Card>();
			foreach (Card card in Enumerable.Reverse<Card>(base.Args.Cards))
			{
				if (base.Battle.AddCardToDrawZone(card, base.Args.DrawZoneTarget) == CancelCause.None)
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

		// Token: 0x06000DBF RID: 3519 RVA: 0x00025CCC File Offset: 0x00023ECC
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.CardsAddedToDrawZone);
		}
	}
}
