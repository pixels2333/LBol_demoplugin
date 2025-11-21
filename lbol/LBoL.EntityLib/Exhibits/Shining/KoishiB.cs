using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200012E RID: 302
	[UsedImplicitly]
	public sealed class KoishiB : ShiningExhibit
	{
		// Token: 0x06000426 RID: 1062 RVA: 0x0000B3CD File Offset: 0x000095CD
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x0000B3EC File Offset: 0x000095EC
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			EnemyType enemyType = base.Battle.EnemyGroup.EnemyType;
			if (enemyType == EnemyType.Elite || enemyType == EnemyType.Boss)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Owner, 0, base.Value1, BlockShieldType.Direct, false);
			}
			yield break;
		}
	}
}
