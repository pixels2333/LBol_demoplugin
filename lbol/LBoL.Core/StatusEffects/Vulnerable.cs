using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class Vulnerable : StatusEffect
	{
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
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
		}
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
