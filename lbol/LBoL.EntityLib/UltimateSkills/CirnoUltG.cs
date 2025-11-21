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
	// Token: 0x02000006 RID: 6
	[UsedImplicitly]
	public sealed class CirnoUltG : UltimateSkill
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000007 RID: 7 RVA: 0x00002107 File Offset: 0x00000307
		[UsedImplicitly]
		public ManaGroup GainMana
		{
			get
			{
				return ManaGroup.Philosophies((base.GameRun != null) ? (base.GameRun.UltimateUseCount + 1) : 1);
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000008 RID: 8 RVA: 0x00002126 File Offset: 0x00000326
		[UsedImplicitly]
		public ManaGroup Mana
		{
			get
			{
				return ManaGroup.Philosophies(1);
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x0000212E File Offset: 0x0000032E
		public CirnoUltG()
		{
			base.TargetType = TargetType.AllEnemies;
			base.GunName = "冰晶绽放";
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002148 File Offset: 0x00000348
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			yield return new DamageAction(base.Owner, selector.GetEnemies(base.Battle), this.Damage, base.GunName, GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new GainManaAction(this.GainMana);
			yield break;
		}
	}
}
