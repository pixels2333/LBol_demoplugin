using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class LockedOn : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnOwnerTurnStarting));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
		}
		private void OnOwnerTurnStarting(UnitEventArgs args)
		{
			if (base.IsAutoDecreasing)
			{
				int num = base.Level - 1;
				base.Level = num;
				if (base.Level == 0)
				{
					this.React(new RemoveStatusEffectAction(this, true, 0.1f));
					return;
				}
			}
			else
			{
				base.IsAutoDecreasing = true;
			}
		}
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			if (damageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = damageInfo.IncreaseBy(base.Level);
				args.AddModifier(this);
			}
		}
	}
}
