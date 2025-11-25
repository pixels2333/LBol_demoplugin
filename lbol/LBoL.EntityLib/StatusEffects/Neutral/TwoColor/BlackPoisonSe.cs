using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	public sealed class BlackPoisonSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<DamageEventArgs>(base.Owner.DamageDealt, new EventSequencedReactor<DamageEventArgs>(this.OnDamageDealt));
		}
		private IEnumerable<BattleAction> OnDamageDealt(DamageEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (args.Target.IsAlive)
			{
				DamageInfo damageInfo = args.DamageInfo;
				if (damageInfo.DamageType == DamageType.Attack && damageInfo.Damage > 0f)
				{
					base.NotifyActivating();
					yield return new ApplyStatusEffectAction<Poison>(args.Target, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			yield break;
		}
	}
}
