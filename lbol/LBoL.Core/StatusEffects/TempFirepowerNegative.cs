using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000B0 RID: 176
	[UsedImplicitly]
	public sealed class TempFirepowerNegative : StatusEffect, IOpposing<TempFirepower>
	{
		// Token: 0x06000810 RID: 2064 RVA: 0x00017CA0 File Offset: 0x00015EA0
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			if (unit is PlayerUnit)
			{
				base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
				{
					this.React(new RemoveStatusEffectAction(this, true, 0.1f));
				});
				return;
			}
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
			{
				if (unit.HasStatusEffect<ExtraTurn>())
				{
					this.React(new RemoveStatusEffectAction(this, true, 0.1f));
				}
			});
			base.HandleOwnerEvent<GameEventArgs>(base.Battle.RoundEnded, delegate(GameEventArgs _)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			});
		}

		// Token: 0x06000811 RID: 2065 RVA: 0x00017D44 File Offset: 0x00015F44
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.ReduceBy(base.Level);
				args.AddModifier(this);
			}
		}

		// Token: 0x06000812 RID: 2066 RVA: 0x00017D84 File Offset: 0x00015F84
		public OpposeResult Oppose(TempFirepower other)
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
