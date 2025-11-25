using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class QuickAct1 : JadeBox
	{
		protected override void OnAdded()
		{
			base.HandleGameRunEvent<GameEventArgs>(base.GameRun.StageEntered, new GameEventHandler<GameEventArgs>(this.OnStageEntered));
		}
		private void OnStageEntered(GameEventArgs args)
		{
			base.GameRun.Damage(Mathf.RoundToInt((float)(base.GameRun.Player.Hp * base.Value1) / 100f), DamageType.HpLose, true, false, null);
		}
		protected override void OnEnterBattle()
		{
			if (base.GameRun.CurrentStation.Act == 1)
			{
				base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStated));
			}
		}
		private IEnumerable<BattleAction> OnBattleStated(GameEventArgs args)
		{
			foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup.Alives)
			{
				if (enemyUnit.IsAlive && enemyUnit.Hp > 1)
				{
					yield return new DamageAction(enemyUnit, enemyUnit, DamageInfo.HpLose((float)enemyUnit.Hp / 2f, false), "Instant", GunType.Single);
				}
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}
	}
}
