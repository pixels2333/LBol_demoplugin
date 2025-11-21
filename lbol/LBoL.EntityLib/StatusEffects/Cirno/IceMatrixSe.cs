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
	// Token: 0x020000E1 RID: 225
	[UsedImplicitly]
	public sealed class IceMatrixSe : StatusEffect
	{
		// Token: 0x0600032A RID: 810 RVA: 0x000087BC File Offset: 0x000069BC
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.EnemySpawned, new GameEventHandler<UnitEventArgs>(this.OnEnemySpawned));
			foreach (EnemyUnit enemyUnit in base.Battle.AllAliveEnemies)
			{
				base.ReactOwnerEvent<StatusEffectApplyEventArgs>(enemyUnit.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnEnemySeAdding));
			}
		}

		// Token: 0x0600032B RID: 811 RVA: 0x0000883C File Offset: 0x00006A3C
		private void OnEnemySpawned(UnitEventArgs args)
		{
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(args.Unit.StatusEffectAdding, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnEnemySeAdding));
		}

		// Token: 0x0600032C RID: 812 RVA: 0x0000885B File Offset: 0x00006A5B
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
