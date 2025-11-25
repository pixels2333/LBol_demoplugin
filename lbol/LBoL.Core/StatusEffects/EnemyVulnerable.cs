using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core.StatusEffects
{
	[UsedImplicitly]
	public sealed class EnemyVulnerable : StatusEffect
	{
		public bool StartAutoDecreasing { get; set; }
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
