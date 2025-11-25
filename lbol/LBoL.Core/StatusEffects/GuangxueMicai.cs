using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class GuangxueMicai : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
		}
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack)
			{
				damageInfo.Damage = damageInfo.Amount * 0.5f;
				args.DamageInfo = damageInfo;
				args.AddModifier(this);
			}
		}
		public override string UnitEffectName
		{
			get
			{
				return "GuangxueMicaiLoop";
			}
		}
	}
}
