using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class Weak : StatusEffect
	{
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
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
		}
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
