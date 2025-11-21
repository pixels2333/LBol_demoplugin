using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A5 RID: 421
	[UsedImplicitly]
	public sealed class Xiangrikui : Exhibit
	{
		// Token: 0x06000607 RID: 1543 RVA: 0x0000E251 File Offset: 0x0000C451
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x0000E275 File Offset: 0x0000C475
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			base.Counter = (base.Counter + 1) % base.Value1;
			if (base.Counter >= base.Value2 && base.Counter <= base.Value3)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			int num = base.Counter + 1;
			base.Active = num >= base.Value2 && num <= base.Value3;
			yield break;
		}

		// Token: 0x06000609 RID: 1545 RVA: 0x0000E285 File Offset: 0x0000C485
		protected override void OnLeaveBattle()
		{
			base.Active = false;
		}
	}
}
