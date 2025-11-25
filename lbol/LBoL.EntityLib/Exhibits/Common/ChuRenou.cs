using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	[ExhibitInfo(ExpireStageLevel = 3, ExpireStationLevel = 9)]
	public sealed class ChuRenou : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsAdding, delegate(CardsEventArgs args)
			{
				if (base.Counter == 0)
				{
					return;
				}
				Card[] cards = args.Cards;
				if (Enumerable.Count<Card>(cards, (Card card) => card.CardType == CardType.Misfortune) > 0)
				{
					List<Card> list = new List<Card>();
					foreach (Card card2 in cards)
					{
						if (card2.CardType == CardType.Misfortune && base.Counter > 0)
						{
							int num = base.Counter - 1;
							base.Counter = num;
							if (base.Counter == 0)
							{
								base.Blackout = true;
							}
						}
						else
						{
							list.Add(card2);
						}
					}
					base.NotifyActivating();
					args.Cards = list.ToArray();
				}
			});
		}
	}
}
