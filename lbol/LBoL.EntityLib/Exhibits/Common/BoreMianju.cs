using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200015A RID: 346
	[UsedImplicitly]
	public sealed class BoreMianju : Exhibit
	{
		// Token: 0x060004C0 RID: 1216 RVA: 0x0000C3E0 File Offset: 0x0000A5E0
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, delegate(UnitEventArgs _)
			{
				base.Counter = 0;
				base.Active = false;
			});
		}

		// Token: 0x060004C1 RID: 1217 RVA: 0x0000C42C File Offset: 0x0000A62C
		protected override void OnLeaveBattle()
		{
			base.Counter = 0;
			base.Active = false;
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x0000C43C File Offset: 0x0000A63C
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
					yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value2), default(int?), default(int?), default(int?), 0f, true);
					base.Active = false;
				}
			}
			yield break;
		}
	}
}
