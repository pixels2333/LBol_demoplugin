using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class FlatPeach : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnDamageReceived));
		}
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0f)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Owner, base.Owner, base.Level, HealType.Normal, 0.2f);
			}
			yield break;
		}
	}
}
