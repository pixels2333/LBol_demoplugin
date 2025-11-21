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
	// Token: 0x0200010E RID: 270
	[UsedImplicitly]
	public sealed class Capital : JadeBox
	{
		// Token: 0x060003B2 RID: 946 RVA: 0x0000A56E File Offset: 0x0000876E
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
		}

		// Token: 0x060003B3 RID: 947 RVA: 0x0000A592 File Offset: 0x00008792
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

		// Token: 0x060003B4 RID: 948 RVA: 0x0000A5A2 File Offset: 0x000087A2
		protected override void OnLeaveBattle()
		{
			base.GameRun.GainMoney(Mathf.CeilToInt((float)(base.GameRun.Money * base.Value3) / 100f), false, null);
		}
	}
}
