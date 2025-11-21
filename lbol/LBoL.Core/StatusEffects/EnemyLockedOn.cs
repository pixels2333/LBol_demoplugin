using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.Core.StatusEffects
{
	// Token: 0x02000095 RID: 149
	[UsedImplicitly]
	public sealed class EnemyLockedOn : StatusEffect
	{
		// Token: 0x1700026F RID: 623
		// (get) Token: 0x06000750 RID: 1872 RVA: 0x00015A9E File Offset: 0x00013C9E
		// (set) Token: 0x06000751 RID: 1873 RVA: 0x00015AA6 File Offset: 0x00013CA6
		public bool StartAutoDecreasing { get; set; }

		// Token: 0x06000752 RID: 1874 RVA: 0x00015AB0 File Offset: 0x00013CB0
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

		// Token: 0x06000753 RID: 1875 RVA: 0x00015B23 File Offset: 0x00013D23
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
			yield return new ApplyStatusEffectAction<LockedOn>(player, num, default(int?), default(int?), default(int?), 0f, startAutoDecreasing);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00015B33 File Offset: 0x00013D33
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
				yield return new ApplyStatusEffectAction<EnemyLockedOn>(unit, num, default(int?), default(int?), num2, 0f, true);
			}
			yield break;
		}
	}
}
