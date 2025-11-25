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
	[UsedImplicitly]
	public sealed class LuoboMoxing : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}
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
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
