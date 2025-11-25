using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class Bingzhilin : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
		}
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Ability)
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => !card.IsForbidden && card.Cost.Any > 0));
				if (list.Count > 0)
				{
					base.NotifyActivating();
					foreach (Card card2 in list.SampleManyOrAll(base.Value1, base.GameRun.BattleRng))
					{
						card2.NotifyActivating();
						card2.DecreaseTurnCost(base.Mana);
					}
				}
			}
		}
	}
}
