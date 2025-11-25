using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class TempFirepowerNegative : StatusEffect, IOpposing<TempFirepower>
	{
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
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.ReduceBy(base.Level);
				args.AddModifier(this);
			}
		}
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
