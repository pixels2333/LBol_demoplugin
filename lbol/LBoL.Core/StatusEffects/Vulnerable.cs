using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000B4 RID: 180
	[UsedImplicitly]
	public sealed class Vulnerable : StatusEffect
	{
		// Token: 0x1700029E RID: 670
		// (get) Token: 0x06000821 RID: 2081 RVA: 0x00018150 File Offset: 0x00016350
		[UsedImplicitly]
		public int Value
		{
			get
			{
				GameRunController gameRun = base.GameRun;
				if (gameRun == null)
				{
					return 50;
				}
				return 50 + ((base.Owner is PlayerUnit) ? gameRun.PlayerVulnerableExtraPercentage : gameRun.EnemyVulnerableExtraPercentage);
			}
		}

		// Token: 0x06000822 RID: 2082 RVA: 0x00018188 File Offset: 0x00016388
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x000181A4 File Offset: 0x000163A4
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack)
			{
				damageInfo.Damage = damageInfo.Amount * (1f + (float)this.Value / 100f);
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}
	}
}
