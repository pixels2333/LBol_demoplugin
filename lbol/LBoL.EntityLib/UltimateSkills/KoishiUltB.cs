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
	// Token: 0x02000008 RID: 8
	[UsedImplicitly]
	public sealed class KoishiUltB : UltimateSkill
	{
		// Token: 0x0600000D RID: 13 RVA: 0x00002190 File Offset: 0x00000390
		public KoishiUltB()
		{
			base.TargetType = TargetType.AllEnemies;
			base.GunName = "心的协奏曲";
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000021AA File Offset: 0x000003AA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return new DamageAction(base.Owner, selector.GetUnits(base.Battle), this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new FollowAttackAction(selector, false);
			yield break;
		}
	}
}
