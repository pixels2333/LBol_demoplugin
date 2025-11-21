using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000B7 RID: 183
	public sealed class LoveGirlDamageIncrease : StatusEffect, IOpposing<LoveGirlDamageReduce>
	{
		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600027D RID: 637 RVA: 0x00006FE6 File Offset: 0x000051E6
		[UsedImplicitly]
		public int Rate
		{
			get
			{
				return base.Level * 20;
			}
		}

		// Token: 0x0600027E RID: 638 RVA: 0x00006FF1 File Offset: 0x000051F1
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0000700C File Offset: 0x0000520C
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * (float)(100 + this.Rate) / 100f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x00007050 File Offset: 0x00005250
		public OpposeResult Oppose(LoveGirlDamageReduce other)
		{
			if (base.Level < other.Level)
			{
				other.Level -= base.Level;
				return OpposeResult.KeepOther;
			}
			if (base.Level == other.Level)
			{
				return OpposeResult.Neutralize;
			}
			base.Level -= other.Level;
			return OpposeResult.KeepSelf;
		}
	}
}
