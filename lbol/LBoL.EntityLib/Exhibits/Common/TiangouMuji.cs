using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200019B RID: 411
	[UsedImplicitly]
	public sealed class TiangouMuji : Exhibit
	{
		// Token: 0x060005D9 RID: 1497 RVA: 0x0000DDC9 File Offset: 0x0000BFC9
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x060005DA RID: 1498 RVA: 0x0000DDE8 File Offset: 0x0000BFE8
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			base.Blackout = true;
			yield break;
		}

		// Token: 0x060005DB RID: 1499 RVA: 0x0000DDF8 File Offset: 0x0000BFF8
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
