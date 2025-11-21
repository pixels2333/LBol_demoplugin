using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Units;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x020000A1 RID: 161
	[UsedImplicitly]
	public sealed class LimitedDamage : StatusEffect
	{
		// Token: 0x06000786 RID: 1926 RVA: 0x000162BC File Offset: 0x000144BC
		protected override void OnAdded(Unit unit)
		{
			this._internalCount = base.Limit;
			base.Count = this._internalCount;
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnTurnStarting));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnDamageReceived));
		}

		// Token: 0x06000787 RID: 1927 RVA: 0x00016329 File Offset: 0x00014529
		private void OnTurnStarting(UnitEventArgs args)
		{
			this._internalCount = base.Limit;
			base.Count = this._internalCount;
			base.Highlight = false;
		}

		// Token: 0x06000788 RID: 1928 RVA: 0x0001634C File Offset: 0x0001454C
		private void OnDamageTaking(DamageEventArgs args)
		{
			int num = args.DamageInfo.Damage.RoundToInt() - this._internalCount;
			if (num > 0)
			{
				args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(num);
				base.NotifyActivating();
				args.AddModifier(this);
			}
			this._internalCount -= args.DamageInfo.Damage.RoundToInt();
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x000163BA File Offset: 0x000145BA
		private void OnDamageReceived(DamageEventArgs args)
		{
			base.Count = Math.Max(this._internalCount, 0);
			if (base.Count == 0)
			{
				base.Highlight = true;
			}
		}

		// Token: 0x04000352 RID: 850
		private int _internalCount;
	}
}
