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
	// Token: 0x02000094 RID: 148
	public sealed class DeathPoison : StatusEffect
	{
		// Token: 0x06000218 RID: 536 RVA: 0x000065C4 File Offset: 0x000047C4
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is EnemyUnit))
			{
				Debug.LogError("Cannot add DeathVulnerable to " + unit.GetType().Name);
			}
			base.ReactOwnerEvent<DieEventArgs>(base.Owner.Dying, new EventSequencedReactor<DieEventArgs>(this.OnDying));
		}

		// Token: 0x06000219 RID: 537 RVA: 0x00006610 File Offset: 0x00004810
		private IEnumerable<BattleAction> OnDying(DieEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Poison>(base.Battle.Player, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
