using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Dolls
{
	// Token: 0x02000258 RID: 600
	[UsedImplicitly]
	public sealed class Shanghai : Doll
	{
		// Token: 0x060009B8 RID: 2488 RVA: 0x00014D98 File Offset: 0x00012F98
		public Shanghai()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "Simple1";
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x060009B9 RID: 2489 RVA: 0x00014DB2 File Offset: 0x00012FB2
		public override int? DownCounter
		{
			get
			{
				return new int?(base.CalculateDamage(this.Damage));
			}
		}

		// Token: 0x060009BA RID: 2490 RVA: 0x00014DC5 File Offset: 0x00012FC5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DamageAction(base.Owner, base.Battle.FirstAliveEnemy, this.Damage, base.GunName, GunType.Single);
			yield break;
		}
	}
}
