using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class Invincible : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
		}
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
		public override string UnitEffectName
		{
			get
			{
				return "InvincibleLoop";
			}
		}
	}
}
