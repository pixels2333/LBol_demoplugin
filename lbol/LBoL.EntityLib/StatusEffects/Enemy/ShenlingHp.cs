using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class ShenlingHp : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			EnemyUnit enemyUnit = unit as EnemyUnit;
			if (enemyUnit != null)
			{
				base.HandleOwnerEvent<DieEventArgs>(enemyUnit.Died, new GameEventHandler<DieEventArgs>(this.OnEnemyDied));
			}
		}
		private void OnEnemyDied(DieEventArgs args)
		{
			base.GameRun.GainMaxHp(base.Level, true, true);
		}
	}
}
