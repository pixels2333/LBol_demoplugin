using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000196 RID: 406
	[UsedImplicitly]
	public sealed class SongjingMuyu : Exhibit
	{
		// Token: 0x060005BF RID: 1471 RVA: 0x0000DB5A File Offset: 0x0000BD5A
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x0000DB79 File Offset: 0x0000BD79
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			if (base.Battle.EnemyGroup.EnemyType == EnemyType.Boss)
			{
				base.NotifyActivating();
				yield return new HealAction(base.Owner, base.Owner, base.GameRun.BaseDeck.Count, HealType.Normal, 0.2f);
			}
			base.Blackout = true;
			yield break;
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x0000DB89 File Offset: 0x0000BD89
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
