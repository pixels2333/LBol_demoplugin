using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class Capital : JadeBox
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
		}
		private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			int num = base.GameRun.Money / base.Value1 * base.Value2;
			if (num > 0)
			{
				yield return new DamageAction(base.Battle.Player, base.Battle.Player, DamageInfo.HpLose((float)num, false), "Instant", GunType.Single);
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.GameRun.GainMoney(Mathf.CeilToInt((float)(base.GameRun.Money * base.Value3) / 100f), false, null);
		}
	}
}
