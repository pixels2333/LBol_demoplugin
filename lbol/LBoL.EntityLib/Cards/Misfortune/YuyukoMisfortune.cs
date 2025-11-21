using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x02000353 RID: 851
	[UsedImplicitly]
	public sealed class YuyukoMisfortune : Card
	{
		// Token: 0x06000C59 RID: 3161 RVA: 0x00018207 File Offset: 0x00016407
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000C5A RID: 3162 RVA: 0x00018226 File Offset: 0x00016426
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Battle.Player.IsInTurn && base.Zone == CardZone.Hand && args.Card != this)
			{
				base.NotifyActivating();
				yield return base.LoseLifeAction(base.Value1);
			}
			yield break;
		}
	}
}
