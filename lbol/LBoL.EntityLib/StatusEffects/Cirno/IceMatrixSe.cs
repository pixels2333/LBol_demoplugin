using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Cirno
{
	[UsedImplicitly]
	public sealed class IceMatrixSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.EnemySpawned, new GameEventHandler<UnitEventArgs>(this.OnEnemySpawned));
			foreach (EnemyUnit enemyUnit in base.Battle.AllAliveEnemies)
			{
				base.ReactOwnerEvent<StatusEffectApplyEventArgs>(enemyUnit.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnEnemySeAdding));
			}
		}
		private void OnEnemySpawned(UnitEventArgs args)
		{
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(args.Unit.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnEnemySeAdding));
		}
		private IEnumerable<BattleAction> OnEnemySeAdding(StatusEffectApplyEventArgs args)
		{
			if (args.Effect is Cold && !base.Battle.BattleShouldEnd && !args.IsCanceled && args.Unit.IsAlive)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<FrostArmor>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
	}
}
