using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000198 RID: 408
	[UsedImplicitly]
	public sealed class TangSan : Exhibit
	{
		// Token: 0x060005CD RID: 1485 RVA: 0x0000DCDE File Offset: 0x0000BEDE
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x0000DD02 File Offset: 0x0000BF02
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new CastBlockShieldAction(base.Owner, base.Owner, base.Value1, 0, BlockShieldType.Normal, true);
				int num;
				for (int i = 0; i < base.Value2; i = num + 1)
				{
					List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card.CanUpgrade && !card.IsUpgraded));
					if (list.Count > 0)
					{
						Card card2 = list.Sample(base.GameRun.BattleRng);
						yield return new UpgradeCardAction(card2);
					}
					num = i;
				}
				base.Blackout = true;
			}
			yield break;
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x0000DD12 File Offset: 0x0000BF12
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
