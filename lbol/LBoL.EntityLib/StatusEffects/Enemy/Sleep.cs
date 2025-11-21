using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000C9 RID: 201
	[UsedImplicitly]
	public sealed class Sleep : StatusEffect
	{
		// Token: 0x060002B7 RID: 695 RVA: 0x000076B0 File Offset: 0x000058B0
		protected override void OnAdded(Unit unit)
		{
			if (unit is EnemyUnit && base.Owner.Shield < base.Level)
			{
				this.React(new CastBlockShieldAction(base.Owner, base.Owner, new ShieldInfo(base.Level - base.Owner.Shield, BlockShieldType.Direct), false));
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnding));
		}

		// Token: 0x060002B8 RID: 696 RVA: 0x0000772A File Offset: 0x0000592A
		private IEnumerable<BattleAction> OnOwnerTurnEnding(UnitEventArgs args)
		{
			if (base.Owner.Shield < base.Level)
			{
				yield return new CastBlockShieldAction(base.Owner, base.Owner, new ShieldInfo(base.Level - base.Owner.Shield, BlockShieldType.Direct), false);
			}
			yield break;
		}
	}
}
