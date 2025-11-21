using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000B5 RID: 181
	[UsedImplicitly]
	public sealed class Weak : StatusEffect
	{
		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06000825 RID: 2085 RVA: 0x000181FC File Offset: 0x000163FC
		[UsedImplicitly]
		public int Value
		{
			get
			{
				GameRunController gameRun = base.GameRun;
				if (gameRun == null || !(base.Owner is EnemyUnit))
				{
					return 30;
				}
				return Math.Min(30 + gameRun.WeakExtraPercentage, 100);
			}
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x00018233 File Offset: 0x00016433
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x00018250 File Offset: 0x00016450
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack)
			{
				damageInfo.Damage = damageInfo.Amount * (1f - (float)this.Value / 100f);
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}
	}
}
