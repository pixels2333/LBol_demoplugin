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
	// Token: 0x02000192 RID: 402
	[UsedImplicitly]
	public sealed class ShoubiaoMazuiqiang : Exhibit
	{
		// Token: 0x060005AD RID: 1453 RVA: 0x0000DA18 File Offset: 0x0000BC18
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x060005AE RID: 1454 RVA: 0x0000DA37 File Offset: 0x0000BC37
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup)
			{
				if (enemyUnit.IsAlive)
				{
					if (base.GameRun.BattleRng.NextInt(0, 1) == 0)
					{
						yield return new ApplyStatusEffectAction<Weak>(enemyUnit, new int?(1), new int?(base.Value1), default(int?), default(int?), 0f, true);
					}
					else
					{
						yield return new ApplyStatusEffectAction<Vulnerable>(enemyUnit, new int?(1), new int?(base.Value1), default(int?), default(int?), 0f, true);
					}
				}
			}
			IEnumerator<EnemyUnit> enumerator = null;
			base.Blackout = true;
			yield break;
			yield break;
		}

		// Token: 0x060005AF RID: 1455 RVA: 0x0000DA47 File Offset: 0x0000BC47
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
