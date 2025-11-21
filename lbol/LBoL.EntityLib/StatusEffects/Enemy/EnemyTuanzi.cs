using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000A1 RID: 161
	public sealed class EnemyTuanzi : StatusEffect
	{
		// Token: 0x0600023E RID: 574 RVA: 0x00006A02 File Offset: 0x00004C02
		protected override void OnAdded(Unit unit)
		{
			if (base.Owner.IsInTurn)
			{
				this._skipFirstTurn = true;
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
		}

		// Token: 0x0600023F RID: 575 RVA: 0x00006A35 File Offset: 0x00004C35
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			if (this._skipFirstTurn)
			{
				this._skipFirstTurn = false;
				yield break;
			}
			Unit owner = base.Owner;
			if (owner != null && owner.IsAlive)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Owner, base.Owner, base.Level, HealType.Normal, 0.2f);
			}
			yield break;
		}

		// Token: 0x04000018 RID: 24
		private bool _skipFirstTurn;
	}
}
