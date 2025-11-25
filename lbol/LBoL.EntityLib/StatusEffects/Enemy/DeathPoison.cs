using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;
using UnityEngine;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	public sealed class DeathPoison : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is EnemyUnit))
			{
				Debug.LogError("Cannot add DeathVulnerable to " + unit.GetType().Name);
			}
			base.ReactOwnerEvent<DieEventArgs>(base.Owner.Dying, new EventSequencedReactor<DieEventArgs>(this.OnDying));
		}
		private IEnumerable<BattleAction> OnDying(DieEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Poison>(base.Battle.Player, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
