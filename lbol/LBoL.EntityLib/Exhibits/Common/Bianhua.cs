using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000157 RID: 343
	[UsedImplicitly]
	public sealed class Bianhua : Exhibit
	{
		// Token: 0x060004B4 RID: 1204 RVA: 0x0000C2EA File Offset: 0x0000A4EA
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<DieEventArgs>(base.Battle.EnemyDied, new EventSequencedReactor<DieEventArgs>(this.OnEnemyDied));
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x0000C309 File Offset: 0x0000A509
		private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
