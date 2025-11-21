using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.UltimateSkills
{
	// Token: 0x02000007 RID: 7
	[UsedImplicitly]
	public sealed class CirnoUltU : UltimateSkill
	{
		// Token: 0x0600000B RID: 11 RVA: 0x0000215F File Offset: 0x0000035F
		public CirnoUltU()
		{
			base.TargetType = TargetType.AllEnemies;
			base.GunName = "完美冻结";
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002179 File Offset: 0x00000379
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return new DamageAction(base.Owner, selector.GetUnits(base.Battle), this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			foreach (Unit unit in selector.GetUnits(base.Battle))
			{
				if (unit.IsAlive)
				{
					yield return new ApplyStatusEffectAction<Cold>(unit, default(int?), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			Unit[] array = null;
			yield break;
		}
	}
}
