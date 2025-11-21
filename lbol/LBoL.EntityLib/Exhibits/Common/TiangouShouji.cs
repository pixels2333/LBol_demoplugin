using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200019E RID: 414
	[UsedImplicitly]
	public sealed class TiangouShouji : Exhibit
	{
		// Token: 0x060005E6 RID: 1510 RVA: 0x0000DEC7 File Offset: 0x0000C0C7
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060005E7 RID: 1511 RVA: 0x0000DEEB File Offset: 0x0000C0EB
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new DrawManyCardAction(base.Value1);
				base.Blackout = true;
			}
			yield break;
		}

		// Token: 0x060005E8 RID: 1512 RVA: 0x0000DEFB File Offset: 0x0000C0FB
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
