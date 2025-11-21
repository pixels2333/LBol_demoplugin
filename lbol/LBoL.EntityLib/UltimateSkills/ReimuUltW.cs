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
	// Token: 0x0200000F RID: 15
	[UsedImplicitly]
	public sealed class ReimuUltW : UltimateSkill
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002295 File Offset: 0x00000495
		[UsedImplicitly]
		public ShieldInfo Shield
		{
			get
			{
				return new ShieldInfo(base.Value2, BlockShieldType.Normal);
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000022A3 File Offset: 0x000004A3
		public ReimuUltW()
		{
			base.TargetType = TargetType.AllEnemies;
			base.GunName = "ReimuSpell2";
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000022BD File Offset: 0x000004BD
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
