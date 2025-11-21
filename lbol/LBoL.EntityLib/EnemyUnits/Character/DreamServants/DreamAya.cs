using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character.DreamServants
{
	// Token: 0x02000250 RID: 592
	[UsedImplicitly]
	public sealed class DreamAya : EnemyUnit
	{
		// Token: 0x06000991 RID: 2449 RVA: 0x0001498C File Offset: 0x00012B8C
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<DreamServant>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<FastAttack>(this, new int?(20), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x06000992 RID: 2450 RVA: 0x00014A0D File Offset: 0x00012C0D
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield return base.AttackMove(base.GetMove(1), base.Gun1, base.Damage1, 2, false, "Instant", false);
			yield break;
		}
	}
}
