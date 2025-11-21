using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.UltimateSkills
{
	// Token: 0x02000005 RID: 5
	public abstract class BaseUlt : UltimateSkill
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000020D6 File Offset: 0x000002D6
		public BaseUlt()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "ReimuSpell1";
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000020F0 File Offset: 0x000002F0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			EnemyUnit enemy = selector.GetEnemy(base.Battle);
			yield return new DamageAction(base.Owner, enemy, this.Damage, base.GunName, GunType.Single);
			yield return new DamageAction(base.Owner, enemy, this.Damage, "Instant", GunType.Single);
			yield return new DamageAction(base.Owner, enemy, this.Damage, "Instant", GunType.Single);
			yield break;
		}
	}
}
