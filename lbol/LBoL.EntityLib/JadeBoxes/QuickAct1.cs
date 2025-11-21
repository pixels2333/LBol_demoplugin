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
	// Token: 0x02000116 RID: 278
	[UsedImplicitly]
	public sealed class QuickAct1 : JadeBox
	{
		// Token: 0x060003D5 RID: 981 RVA: 0x0000AA8D File Offset: 0x00008C8D
		protected override void OnAdded()
		{
			base.HandleGameRunEvent<GameEventArgs>(base.GameRun.StageEntered, new GameEventHandler<GameEventArgs>(this.OnStageEntered));
		}

		// Token: 0x060003D6 RID: 982 RVA: 0x0000AAAC File Offset: 0x00008CAC
		private void OnStageEntered(GameEventArgs args)
		{
			base.GameRun.Damage(Mathf.RoundToInt((float)(base.GameRun.Player.Hp * base.Value1) / 100f), DamageType.HpLose, true, false, null);
		}

		// Token: 0x060003D7 RID: 983 RVA: 0x0000AAE0 File Offset: 0x00008CE0
		protected override void OnEnterBattle()
		{
			if (base.GameRun.CurrentStation.Act == 1)
			{
				base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStated));
			}
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x0000AB12 File Offset: 0x00008D12
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
