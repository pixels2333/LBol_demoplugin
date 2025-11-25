using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Others
{
	[UsedImplicitly]
	public sealed class CatchGoblinSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DieEventArgs>(base.Owner.Died, new EventSequencedReactor<DieEventArgs>(this.OnOwnerDied));
			base.HandleOwnerEvent<DamageEventArgs>(unit.DamageReceiving, new GameEventHandler<DamageEventArgs>(this.OnDamageReceiving));
			base.Highlight = true;
		}
		private IEnumerable<BattleAction> OnOwnerDied(DieEventArgs args)
		{
			yield return new GainMoneyAction(50, SpecialSourceType.None);
			yield break;
		}
		private void OnDamageReceiving(DamageEventArgs args)
		{
			DamageInfo damageInfo = args.DamageInfo;
			damageInfo.Damage = damageInfo.Amount * 0.66f;
			args.DamageInfo = damageInfo;
			args.AddModifier(this);
		}
	}
}
