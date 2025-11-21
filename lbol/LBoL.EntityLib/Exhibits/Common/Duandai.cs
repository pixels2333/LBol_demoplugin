using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000163 RID: 355
	[UsedImplicitly]
	public sealed class Duandai : Exhibit
	{
		// Token: 0x060004E5 RID: 1253 RVA: 0x0000C744 File Offset: 0x0000A944
		protected override void OnEnterBattle()
		{
			EnemyType enemyType = base.Battle.EnemyGroup.EnemyType;
			if (enemyType == EnemyType.Elite || enemyType == EnemyType.Boss)
			{
				base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
			}
		}

		// Token: 0x060004E6 RID: 1254 RVA: 0x0000C78C File Offset: 0x0000A98C
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
