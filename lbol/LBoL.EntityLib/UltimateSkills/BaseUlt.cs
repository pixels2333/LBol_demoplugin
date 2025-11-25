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
	public abstract class BaseUlt : UltimateSkill
	{
		public BaseUlt()
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
