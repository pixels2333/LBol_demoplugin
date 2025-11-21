using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000C7 RID: 199
	[UsedImplicitly]
	public sealed class ShenlingHp : StatusEffect
	{
		// Token: 0x060002B2 RID: 690 RVA: 0x00007624 File Offset: 0x00005824
		protected override void OnAdded(Unit unit)
		{
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				base.HandleOwnerEvent<DieEventArgs>(enemyUnit.Died, new GameEventHandler<DieEventArgs>(this.OnEnemyDied));
			}
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x00007653 File Offset: 0x00005853
		private void OnEnemyDied(DieEventArgs args)
		{
			base.GameRun.GainMaxHp(base.Level, true, true);
		}
	}
}
