using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class LoveGirlDamageReduce : StatusEffect, IOpposing<LoveGirlDamageIncrease>
	{
		[UsedImplicitly]
		public int Rate
		{
			get
			{
				return Math.Min(100, base.Level * 20);
			}
		}
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
			this.React(PerformAction.EffectMessage(unit, "LoveGirlEffectManager", "Add", base.Level));
		}
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * (float)(100 - this.Rate) / 100f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
		public OpposeResult Oppose(LoveGirlDamageIncrease other)
		{
			this.React(PerformAction.EffectMessage(base.Owner, "LoveGirlEffectManager", "Remove", other.Level));
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
