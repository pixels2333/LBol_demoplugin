using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B9 RID: 441
	[UsedImplicitly]
	public sealed class Zuoshan : Exhibit
	{
		// Token: 0x17000081 RID: 129
		// (get) Token: 0x06000658 RID: 1624 RVA: 0x0000EA73 File Offset: 0x0000CC73
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

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000659 RID: 1625 RVA: 0x0000EA94 File Offset: 0x0000CC94
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600065A RID: 1626 RVA: 0x0000EA97 File Offset: 0x0000CC97
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new GameEventHandler<GameEventArgs>(this.OnBattleStarted));
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x0600065B RID: 1627 RVA: 0x0000EAD3 File Offset: 0x0000CCD3
		private void OnBattleStarted(GameEventArgs args)
		{
			base.Counter = 0;
		}

		// Token: 0x0600065C RID: 1628 RVA: 0x0000EADC File Offset: 0x0000CCDC
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

		// Token: 0x0600065D RID: 1629 RVA: 0x0000EAF3 File Offset: 0x0000CCF3
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
