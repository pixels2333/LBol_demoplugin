using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(AliceBook.AliceBookWeighter))]
	public sealed class AliceBook : Exhibit
	{
		protected override void OnEnterBattle()
		{
			this.SetMana(base.Battle.EnumerateAllCards());
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToDiscard, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToHand, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsEventArgs>(base.Battle.CardsAddedToExile, new GameEventHandler<CardsEventArgs>(this.OnAddCard));
			base.HandleBattleEvent<CardsAddingToDrawZoneEventArgs>(base.Battle.CardsAddedToDrawZone, new GameEventHandler<CardsAddingToDrawZoneEventArgs>(this.OnAddCardToDraw));
		}
		private void OnAddCard(CardsEventArgs args)
		{
			this.SetMana(args.Cards);
		}
		private void OnAddCardToDraw(CardsAddingToDrawZoneEventArgs args)
		{
			this.SetMana(args.Cards);
		}
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
		private class AliceBookWeighter : IExhibitWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)(Enumerable.Any<Card>(gameRun.BaseDeck, (Card card) => card.HasKicker) ? 1 : 0);
			}
		}
	}
}
