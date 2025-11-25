using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core.Units;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class LimitedDamage : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			this._internalCount = base.Limit;
			base.Count = this._internalCount;
			base.HandleOwnerEvent<UnitEventArgs>(unit.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnTurnStarting));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnDamageTaking));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnDamageReceived));
		}
		private void OnTurnStarting(UnitEventArgs args)
		{
			this._internalCount = base.Limit;
			base.Count = this._internalCount;
			base.Highlight = false;
		}
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
		private void OnDamageReceived(DamageEventArgs args)
		{
			base.Count = Math.Max(this._internalCount, 0);
			if (base.Count == 0)
			{
				base.Highlight = true;
			}
		}
		private int _internalCount;
	}
}
