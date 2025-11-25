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
	[UsedImplicitly]
	public sealed class ShoubiaoMazuiqiang : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}
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
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
