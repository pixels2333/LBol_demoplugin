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
	// Token: 0x02000257 RID: 599
	[UsedImplicitly]
	public sealed class Penglai : Doll
	{
		// Token: 0x060009B3 RID: 2483 RVA: 0x00014D3B File Offset: 0x00012F3B
		public Penglai()
		{
			base.TargetType = TargetType.SingleEnemy;
			base.GunName = "Simple1";
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x060009B4 RID: 2484 RVA: 0x00014D55 File Offset: 0x00012F55
		public override int? DownCounter
		{
			get
			{
				return new int?(base.CalculateDamage(this.Damage));
			}
		}

		// Token: 0x060009B5 RID: 2485 RVA: 0x00014D68 File Offset: 0x00012F68
		protected override IEnumerable<BattleAction> ActiveActions()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DamageAction(base.Owner, base.Battle.LowestHpEnemy, this.Damage, "Simple1", GunType.Single);
			yield break;
		}

		// Token: 0x060009B6 RID: 2486 RVA: 0x00014D78 File Offset: 0x00012F78
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DamageAction(base.Owner, base.Battle.LowestHpEnemy, this.Damage, "Simple1", GunType.Single);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new RemoveDollAction(this);
			yield break;
		}

		// Token: 0x060009B7 RID: 2487 RVA: 0x00014D88 File Offset: 0x00012F88
		public override IEnumerable<BattleAction> OnRemove()
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Magic > 0)
			{
				yield return new GainManaAction(base.Mana * base.Magic);
			}
			yield break;
		}
	}
}
