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
	// Token: 0x0200000B RID: 11
	[UsedImplicitly]
	public sealed class MarisaUltR : UltimateSkill
	{
		// Token: 0x06000013 RID: 19 RVA: 0x00002223 File Offset: 0x00000423
		public MarisaUltR()
		{
			base.TargetType = TargetType.AllEnemies;
			base.GunName = "MasterSpark";
		}

		// Token: 0x06000014 RID: 20 RVA: 0x0000223D File Offset: 0x0000043D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return new DamageAction(base.Owner, selector.GetUnits(base.Battle), this.Damage, base.GunName, GunType.Single);
			yield break;
		}
	}
}
