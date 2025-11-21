using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000154 RID: 340
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(AliceBook.AliceBookWeighter))]
	public sealed class AliceBook : Exhibit
	{
		// Token: 0x060004A1 RID: 1185 RVA: 0x0000C00C File Offset: 0x0000A20C
		protected override void OnEnterBattle()
		{
			this.SetMana(base.Battle.EnumerateAllCards());
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x0000C09E File Offset: 0x0000A29E
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x0000C0AC File Offset: 0x0000A2AC
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetMana(args.Cards);
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x0000C0BC File Offset: 0x0000A2BC
		private void SetMana(IEnumerable<Card> cards)
		{
			bool flag = true;
			foreach (Card card in cards)
			{
				if (card.HasKicker)
				{
					if (flag)
					{
						base.NotifyActivating();
						flag = false;
					}
					card.KickerDelta -= base.Mana;
				}
			}
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x0000C12C File Offset: 0x0000A32C
		protected override void OnLeaveBattle()
		{
			foreach (Card card in base.Battle.EnumerateAllCards())
			{
				if (card.HasKicker)
				{
					card.KickerDelta += base.Mana;
				}
			}
		}

		// Token: 0x0200062D RID: 1581
		private class AliceBookWeighter : IExhibitWeighter
		{
			// Token: 0x060018E4 RID: 6372 RVA: 0x00032B33 File Offset: 0x00030D33
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)(Enumerable.Any<Card>(gameRun.BaseDeck, (Card card) => card.HasKicker) ? 1 : 0);
			}
		}
	}
}
