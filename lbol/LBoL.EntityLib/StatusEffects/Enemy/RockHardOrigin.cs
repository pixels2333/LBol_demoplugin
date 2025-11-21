using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000C4 RID: 196
	[UsedImplicitly]
	public abstract class RockHardOrigin : StatusEffect
	{
		// Token: 0x060002A7 RID: 679 RVA: 0x000074E0 File Offset: 0x000056E0
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x000074FC File Offset: 0x000056FC
		private void OnDamageTaking(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack && damageInfo.Damage > 0f)
			{
				args.DamageInfo = damageInfo.ReduceActualDamageBy(base.Level);
				base.NotifyActivating();
				args.AddModifier(this);
			}
		}
	}
}
