using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000A2 RID: 162
	[UsedImplicitly]
	public sealed class LockedOn : StatusEffect
	{
		// Token: 0x0600078B RID: 1931 RVA: 0x000163E5 File Offset: 0x000145E5
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(base.Owner.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnOwnerTurnStarting));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x0001641C File Offset: 0x0001461C
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

		// Token: 0x0600078D RID: 1933 RVA: 0x00016468 File Offset: 0x00014668
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
