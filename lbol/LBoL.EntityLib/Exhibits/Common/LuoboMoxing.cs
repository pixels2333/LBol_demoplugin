using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200017E RID: 382
	[UsedImplicitly]
	public sealed class LuoboMoxing : Exhibit
	{
		// Token: 0x0600055C RID: 1372 RVA: 0x0000D23B File Offset: 0x0000B43B
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x0000D25A File Offset: 0x0000B45A
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			EnemyType enemyType = base.Battle.EnemyGroup.EnemyType;
			if (enemyType == EnemyType.Elite || enemyType == EnemyType.Boss)
			{
				base.NotifyActivating();
				yield return new ApplyStatusEffectAction<Firepower>(base.Owner, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
			}
			base.Blackout = true;
			yield break;
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0000D26A File Offset: 0x0000B46A
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
