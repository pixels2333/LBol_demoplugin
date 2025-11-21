using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200019F RID: 415
	[UsedImplicitly]
	public sealed class TiangouXiangji : Exhibit
	{
		// Token: 0x060005EA RID: 1514 RVA: 0x0000DF0C File Offset: 0x0000C10C
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x0000DF2B File Offset: 0x0000C12B
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup)
			{
				if (enemyUnit.IsAlive)
				{
					yield return new ApplyStatusEffectAction<LockedOn>(enemyUnit, new int?(base.Value1), default(int?), default(int?), default(int?), 0f, true);
				}
			}
			IEnumerator<EnemyUnit> enumerator = null;
			base.Blackout = true;
			yield break;
			yield break;
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x0000DF3B File Offset: 0x0000C13B
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
