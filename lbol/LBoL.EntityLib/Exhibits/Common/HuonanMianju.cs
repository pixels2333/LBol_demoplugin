using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000173 RID: 371
	[UsedImplicitly]
	public sealed class HuonanMianju : Exhibit
	{
		// Token: 0x06000528 RID: 1320 RVA: 0x0000CD64 File Offset: 0x0000AF64
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, delegate(UnitEventArgs _)
			{
				base.Counter = 0;
				base.Active = false;
			});
		}

		// Token: 0x06000529 RID: 1321 RVA: 0x0000CDB0 File Offset: 0x0000AFB0
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
			base.Active = false;
		}

		// Token: 0x0600052A RID: 1322 RVA: 0x0000CDC0 File Offset: 0x0000AFC0
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
					yield return new CastBlockShieldAction(base.Owner, base.Value2, 0, BlockShieldType.Normal, true);
				}
			}
			yield break;
		}
	}
}
