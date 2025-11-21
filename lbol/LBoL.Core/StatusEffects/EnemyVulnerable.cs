using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x02000096 RID: 150
	[UsedImplicitly]
	public sealed class EnemyVulnerable : StatusEffect
	{
		// Token: 0x17000270 RID: 624
		// (get) Token: 0x06000756 RID: 1878 RVA: 0x00015B4B File Offset: 0x00013D4B
		// (set) Token: 0x06000757 RID: 1879 RVA: 0x00015B53 File Offset: 0x00013D53
		public bool StartAutoDecreasing { get; set; }

		// Token: 0x06000758 RID: 1880 RVA: 0x00015B5C File Offset: 0x00013D5C
		protected override void OnAdded(Unit unit)
		{
			if (unit is PlayerUnit)
			{
				Debug.LogWarning(this.DebugName + " should not add to player.");
			}
			this.StartAutoDecreasing = base.Limit != 0;
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnEnded, new EventSequencedReactor<GameEventArgs>(this.OnAllEnemyTurnEnded));
			base.ReactOwnerEvent<DieEventArgs>(base.Owner.Dying, new EventSequencedReactor<DieEventArgs>(this.OnOwnerDying));
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x00015BCF File Offset: 0x00013DCF
		private IEnumerable<BattleAction> OnAllEnemyTurnEnded(GameEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			Unit player = base.Battle.Player;
			int? num = new int?(base.Level);
			bool startAutoDecreasing = this.StartAutoDecreasing;
			yield return new ApplyStatusEffectAction<Vulnerable>(player, default(int?), num, default(int?), default(int?), 0f, startAutoDecreasing);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x00015BDF File Offset: 0x00013DDF
		private IEnumerable<BattleAction> OnOwnerDying(DieEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			EnemyUnit lastAliveEnemy = base.Battle.LastAliveEnemy;
			if (lastAliveEnemy != null && lastAliveEnemy != base.Owner)
			{
				Unit unit = lastAliveEnemy;
				int? num = new int?(base.Level);
				int? num2 = new int?(this.StartAutoDecreasing ? 1 : 0);
				yield return new ApplyStatusEffectAction<EnemyVulnerable>(unit, num, default(int?), default(int?), num2, 0f, true);
			}
			yield break;
		}
	}
}
