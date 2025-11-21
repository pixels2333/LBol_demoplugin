using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Cirno;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000123 RID: 291
	[UsedImplicitly]
	public sealed class CirnoG : ShiningExhibit
	{
		// Token: 0x06000400 RID: 1024 RVA: 0x0000B021 File Offset: 0x00009221
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000401 RID: 1025 RVA: 0x0000B040 File Offset: 0x00009240
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			Card card = args.Card;
			if (card.CardType == CardType.Friend && card.Summoning)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<SummerFlower>(base.Value1, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
