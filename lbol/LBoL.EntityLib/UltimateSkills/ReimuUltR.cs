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
	[UsedImplicitly]
	public sealed class ReimuUltR : UltimateSkill
	{
		public ReimuUltR()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "ReimuSpell1";
		}
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
