using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000180 RID: 384
	[UsedImplicitly]
	public sealed class Mengriji : Exhibit
	{
		// Token: 0x06000563 RID: 1379 RVA: 0x0000D2B8 File Offset: 0x0000B4B8
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x06000564 RID: 1380 RVA: 0x0000D304 File Offset: 0x0000B504
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Defense)
			{
				base.Active = false;
			}
		}

		// Token: 0x06000565 RID: 1381 RVA: 0x0000D31B File Offset: 0x0000B51B
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Active)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			base.Active = true;
			yield break;
		}

		// Token: 0x06000566 RID: 1382 RVA: 0x0000D32B File Offset: 0x0000B52B
		protected override void OnLeaveBattle()
		{
			base.Active = false;
		}
	}
}
