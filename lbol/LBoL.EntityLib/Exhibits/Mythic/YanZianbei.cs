using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Mythic
{
	// Token: 0x02000153 RID: 339
	[UsedImplicitly]
	public sealed class YanZianbei : MythicExhibit
	{
		// Token: 0x0600049E RID: 1182 RVA: 0x0000BFD0 File Offset: 0x0000A1D0
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x0600049F RID: 1183 RVA: 0x0000BFF4 File Offset: 0x0000A1F4
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs arg)
		{
			base.NotifyActivating();
			yield return new HealAction(base.Owner, base.Owner, base.Value1, HealType.Normal, 0.2f);
			yield break;
		}
	}
}
