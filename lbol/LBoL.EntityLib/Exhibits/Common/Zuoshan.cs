using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Zuoshan : Exhibit
	{
		public override string OverrideIconName
		{
			get
			{
				if (base.Counter != 0)
				{
					return base.Id + "Inactive";
				}
				return base.Id;
			}
		}
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new GameEventHandler<GameEventArgs>(this.OnBattleStarted));
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private void OnBattleStarted(GameEventArgs args)
		{
			base.Counter = 0;
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Counter != 0)
			{
				yield break;
			}
			Card card = args.Card;
			if (!card.CanBeDuplicated)
			{
				yield break;
			}
			base.Counter = 1;
			base.NotifyActivating();
			Card card2 = card.CloneBattleCard();
			card2.SetTurnCost(base.Mana);
			card2.IsExile = true;
			List<Card> list = new List<Card>();
			list.Add(card2);
			List<Card> list2 = list;
			yield return new AddCardsToHandAction(list2, AddCardsType.Normal);
			base.Blackout = true;
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
