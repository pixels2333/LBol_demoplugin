using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class LoveGirlDamageIncrease : StatusEffect, IOpposing<LoveGirlDamageReduce>
	{
		[UsedImplicitly]
		public int Rate
		{
			get
			{
				return base.Level * 20;
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
		}
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * (float)(100 + this.Rate) / 100f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
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
