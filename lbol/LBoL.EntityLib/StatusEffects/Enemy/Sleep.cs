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
	[UsedImplicitly]
	public sealed class Sleep : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			if (unit is EnemyUnit && base.Owner.Shield < base.Level)
			{
				this.React(new CastBlockShieldAction(base.Owner, base.Owner, new ShieldInfo(base.Level - base.Owner.Shield, BlockShieldType.Direct), false));
			}
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnding));
		}
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
