using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000B8 RID: 184
	public sealed class LoveGirlDamageReduce : StatusEffect, IOpposing<LoveGirlDamageIncrease>
	{
		// Token: 0x1700003F RID: 63
		// (get) Token: 0x06000282 RID: 642 RVA: 0x000070AC File Offset: 0x000052AC
		[UsedImplicitly]
		public int Rate
		{
			get
			{
				return Math.Min(100, base.Level * 20);
			}
		}

		// Token: 0x06000283 RID: 643 RVA: 0x000070BE File Offset: 0x000052BE
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
			this.React(PerformAction.EffectMessage(unit, "LoveGirlEffectManager", "Add", base.Level));
		}

		// Token: 0x06000284 RID: 644 RVA: 0x00007100 File Offset: 0x00005300
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * (float)(100 - this.Rate) / 100f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}

		// Token: 0x06000285 RID: 645 RVA: 0x00007144 File Offset: 0x00005344
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
