using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Adventure
{
	// Token: 0x020001C0 RID: 448
	[UsedImplicitly]
	public sealed class JingesiYushou : Exhibit
	{
		// Token: 0x06000678 RID: 1656 RVA: 0x0000EDC3 File Offset: 0x0000CFC3
		protected override void OnEnterBattle()
		{
			base.Active = true;
			base.ReactBattleEvent<StatusEffectApplyEventArgs>(base.GameRun.Player.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnStatusEffectAdding));
		}

		// Token: 0x06000679 RID: 1657 RVA: 0x0000EDEE File Offset: 0x0000CFEE
		private IEnumerable<BattleAction> OnStatusEffectAdding(StatusEffectApplyEventArgs args)
		{
			if (base.Active && args.Effect.Type == StatusEffectType.Negative)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Battle.Player, 0, base.Value1, BlockShieldType.Normal, false);
				base.Active = false;
				base.Blackout = true;
			}
			yield break;
		}

		// Token: 0x0600067A RID: 1658 RVA: 0x0000EE05 File Offset: 0x0000D005
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
	}
}
