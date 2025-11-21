using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A7 RID: 423
	[UsedImplicitly]
	public sealed class XijianSan : Exhibit
	{
		// Token: 0x06000610 RID: 1552 RVA: 0x0000E2EA File Offset: 0x0000C4EA
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000611 RID: 1553 RVA: 0x0000E30E File Offset: 0x0000C50E
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}

		// Token: 0x06000612 RID: 1554 RVA: 0x0000E31E File Offset: 0x0000C51E
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 2)
			{
				base.Active = true;
			}
			if (base.Battle.Player.TurnCounter == 3)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Owner, base.Owner, base.Value1, 0, BlockShieldType.Normal, true);
				yield return new GainManaAction(base.Mana);
				base.Active = false;
				base.Blackout = true;
			}
			yield break;
		}
	}
}
