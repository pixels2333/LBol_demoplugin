using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200019D RID: 413
	[UsedImplicitly]
	public sealed class TiangouShanzi : Exhibit
	{
		// Token: 0x060005E1 RID: 1505 RVA: 0x0000DE4C File Offset: 0x0000C04C
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				base.Blackout = true;
			});
		}

		// Token: 0x060005E2 RID: 1506 RVA: 0x0000DE9D File Offset: 0x0000C09D
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x060005E3 RID: 1507 RVA: 0x0000DEAD File Offset: 0x0000C0AD
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
