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
	// Token: 0x02000099 RID: 153
	[UsedImplicitly]
	public sealed class DroneBlock : StatusEffect
	{
		// Token: 0x06000228 RID: 552 RVA: 0x00006760 File Offset: 0x00004960
		protected override void OnAdded(Unit unit)
		{
			if (unit is EnemyUnit)
			{
				this.React(new CastBlockShieldAction(base.Owner, base.Owner, new BlockInfo(base.Level, BlockShieldType.Direct), false));
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnding));
		}

		// Token: 0x06000229 RID: 553 RVA: 0x000067BB File Offset: 0x000049BB
		private IEnumerable<BattleAction> OnOwnerTurnEnding(UnitEventArgs args)
		{
			yield return new CastBlockShieldAction(base.Owner, base.Owner, new BlockInfo(base.Level, BlockShieldType.Direct), false);
			yield break;
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x0600022A RID: 554 RVA: 0x000067CB File Offset: 0x000049CB
		public override string UnitEffectName
		{
			get
			{
				return "DroneBlockLoop";
			}
		}
	}
}
