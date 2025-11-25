using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.UltimateSkills
{
	[UsedImplicitly]
	public sealed class ReimuUltW : UltimateSkill
	{
		[UsedImplicitly]
		public ShieldInfo Shield
		{
			get
			{
				return new ShieldInfo(base.Value2, BlockShieldType.Normal);
			}
		}
		public ReimuUltW()
		{
			base.TargetType = TargetType.AllEnemies;
			base.GunName = "ReimuSpell2";
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			Unit[] targets = selector.GetUnits(base.Battle);
			yield return PerformAction.Spell(base.Owner, "八方鬼缚阵");
			yield return new CastBlockShieldAction(base.Owner, base.Owner, this.Shield, false);
			yield return new DamageAction(base.Owner, targets, this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (Unit unit in targets)
			{
				if (unit.IsAlive)
				{
					yield return new ApplyStatusEffectAction<Weak>(unit, default(int?), new int?(base.Value1), default(int?), default(int?), 0f, true);
				}
			}
			Unit[] array = null;
			yield break;
		}
	}
}
