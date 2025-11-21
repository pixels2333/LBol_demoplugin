using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character.DreamServants
{
	// Token: 0x02000251 RID: 593
	[UsedImplicitly]
	public sealed class DreamJunko : EnemyUnit
	{
		// Token: 0x06000994 RID: 2452 RVA: 0x00014A28 File Offset: 0x00012C28
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<DreamServant>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<JunkoPurify>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x00014AA8 File Offset: 0x00012CA8
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield return base.AttackMove(base.GetSpellCardName(default(int?), 1), base.Gun1, base.Damage1, 6, false, "Instant", true);
			yield break;
		}
	}
}
