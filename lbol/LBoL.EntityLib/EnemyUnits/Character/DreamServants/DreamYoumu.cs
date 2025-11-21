using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character.DreamServants
{
	// Token: 0x02000253 RID: 595
	[UsedImplicitly]
	public sealed class DreamYoumu : EnemyUnit
	{
		// Token: 0x0600099A RID: 2458 RVA: 0x00014B60 File Offset: 0x00012D60
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<DreamServant>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<LouguanJianSe>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x00014BE0 File Offset: 0x00012DE0
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield return base.AttackMove(base.GetSpellCardName(default(int?), 0), base.Gun1, base.Damage1, 3, false, base.Gun1, false);
			yield break;
		}
	}
}
