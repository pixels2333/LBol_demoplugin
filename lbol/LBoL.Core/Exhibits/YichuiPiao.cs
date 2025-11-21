using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.Core.Exhibits
{
	// Token: 0x0200011C RID: 284
	[UsedImplicitly]
	public sealed class YichuiPiao : Exhibit
	{
		// Token: 0x06000A1C RID: 2588 RVA: 0x0001CA98 File Offset: 0x0001AC98
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.YichuiPiaoFlag + 1;
			gameRun.YichuiPiaoFlag = num;
		}

		// Token: 0x06000A1D RID: 2589 RVA: 0x0001CABC File Offset: 0x0001ACBC
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.YichuiPiaoFlag - 1;
			gameRun.YichuiPiaoFlag = num;
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x0001CADE File Offset: 0x0001ACDE
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x0001CB02 File Offset: 0x0001AD02
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<Xiaozhuo>(base.Value1, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
