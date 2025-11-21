using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000AF RID: 175
	[UsedImplicitly]
	public sealed class TempFirepower : StatusEffect, IOpposing<TempFirepowerNegative>
	{
		// Token: 0x0600080B RID: 2059 RVA: 0x00017BB4 File Offset: 0x00015DB4
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<DamageDealingEventArgs>(unit.DamageDealing, new GameEventHandler<DamageDealingEventArgs>(this.OnDamageDealing));
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnEnded, delegate(UnitEventArgs _)
			{
				this.React(new RemoveStatusEffectAction(this, true, 0.1f));
			});
		}

		// Token: 0x0600080C RID: 2060 RVA: 0x00017BE8 File Offset: 0x00015DE8
		private void OnDamageDealing(DamageDealingEventArgs args)
		{
			if (args.DamageInfo.DamageType == DamageType.Attack)
			{
				args.DamageInfo = args.DamageInfo.IncreaseBy(base.Level);
				args.AddModifier(this);
			}
		}

		// Token: 0x0600080D RID: 2061 RVA: 0x00017C28 File Offset: 0x00015E28
		public OpposeResult Oppose(TempFirepowerNegative other)
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
