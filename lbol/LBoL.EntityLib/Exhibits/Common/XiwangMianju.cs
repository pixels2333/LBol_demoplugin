using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A8 RID: 424
	[UsedImplicitly]
	public sealed class XiwangMianju : Exhibit
	{
		// Token: 0x06000614 RID: 1556 RVA: 0x0000E338 File Offset: 0x0000C538
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, delegate(UnitEventArgs _)
			{
				base.Counter = 0;
				base.Active = false;
			});
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x0000E384 File Offset: 0x0000C584
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
			base.Active = false;
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x0000E394 File Offset: 0x0000C594
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn && args.Card.CardType == CardType.Attack)
			{
				base.Counter = (base.Counter + 1) % base.Value1;
				if (base.Counter == 1)
				{
					base.Active = true;
				}
				if (base.Counter == 0)
				{
					base.NotifyActivating();
					yield return new GainPowerAction(base.Value2);
				}
			}
			yield break;
		}
	}
}
