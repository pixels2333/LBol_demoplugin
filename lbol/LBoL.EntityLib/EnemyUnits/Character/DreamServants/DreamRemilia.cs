using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character.DreamServants
{
	// Token: 0x02000252 RID: 594
	[UsedImplicitly]
	public sealed class DreamRemilia : EnemyUnit
	{
		// Token: 0x06000997 RID: 2455 RVA: 0x00014AC0 File Offset: 0x00012CC0
		public override void OnSpawn(EnemyUnit spawner)
		{
			int? num = default(int?);
			int? num2 = default(int?);
			int? num3 = default(int?);
			int? num4 = default(int?);
			this.React(new ApplyStatusEffectAction<DreamServant>(this, num, num2, num3, num4, 0f, true));
			num4 = new int?(base.Count2);
			this.React(new ApplyStatusEffectAction<ScarletDestiny>(this, default(int?), default(int?), default(int?), num4, 0f, true));
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x00014B48 File Offset: 0x00012D48
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			yield return base.AttackMove(base.GetSpellCardName(default(int?), 2), base.Gun1, base.Damage1, 2, false, "Instant", true);
			yield break;
		}
	}
}
