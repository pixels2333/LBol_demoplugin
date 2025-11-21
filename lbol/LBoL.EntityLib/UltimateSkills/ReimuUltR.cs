using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.UltimateSkills
{
	// Token: 0x0200000E RID: 14
	[UsedImplicitly]
	public sealed class ReimuUltR : UltimateSkill
	{
		// Token: 0x06000017 RID: 23 RVA: 0x00002264 File Offset: 0x00000464
		public ReimuUltR()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "ReimuSpell1";
		}

		// Token: 0x06000018 RID: 24 RVA: 0x0000227E File Offset: 0x0000047E
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
