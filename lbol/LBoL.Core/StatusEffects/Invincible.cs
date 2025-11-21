using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x0200009E RID: 158
	[UsedImplicitly]
	public sealed class Invincible : StatusEffect
	{
		// Token: 0x0600077D RID: 1917 RVA: 0x000161DF File Offset: 0x000143DF
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x000161FC File Offset: 0x000143FC
		private void OnDamageTaking(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			int num = damageInfo.Damage.RoundToInt();
			if (num > 1)
			{
				base.NotifyActivating();
				args.DamageInfo = damageInfo.ReduceActualDamageBy(num - 1);
				args.AddModifier(this);
			}
		}

		// Token: 0x17000276 RID: 630
		// (get) Token: 0x0600077F RID: 1919 RVA: 0x0001623E File Offset: 0x0001443E
		public override string UnitEffectName
		{
			get
			{
				return "InvincibleLoop";
			}
		}
	}
}
