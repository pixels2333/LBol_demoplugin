using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x02000095 RID: 149
	public sealed class DeathVulnerable : StatusEffect
	{
		// Token: 0x0600021B RID: 539 RVA: 0x00006628 File Offset: 0x00004828
		protected override void OnAdded(Unit unit)
		{
			if (!(unit is EnemyUnit))
			{
				Debug.LogError("Cannot add DeathVulnerable to " + unit.GetType().Name);
			}
			base.ReactOwnerEvent<DieEventArgs>(base.Owner.Dying, new EventSequencedReactor<DieEventArgs>(this.OnDying));
		}

		// Token: 0x0600021C RID: 540 RVA: 0x00006674 File Offset: 0x00004874
		private IEnumerable<BattleAction> OnDying(DieEventArgs args)
		{
			base.NotifyActivating();
			Unit player = base.Battle.Player;
			int? num = new int?(base.Level);
			yield return new ApplyStatusEffectAction<Vulnerable>(player, default(int?), num, default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
