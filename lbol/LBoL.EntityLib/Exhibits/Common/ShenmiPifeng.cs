using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200018E RID: 398
	[UsedImplicitly]
	public sealed class ShenmiPifeng : Exhibit
	{
		// Token: 0x0600059E RID: 1438 RVA: 0x0000D8EA File Offset: 0x0000BAEA
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x0000D90E File Offset: 0x0000BB0E
		private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
		{
			int num = base.Battle.HandZone.Count * base.Value1;
			if (num > 0)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Battle.Player, num, 0, BlockShieldType.Normal, true);
			}
			yield break;
		}
	}
}
