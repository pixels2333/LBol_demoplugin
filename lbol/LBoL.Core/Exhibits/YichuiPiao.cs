using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.Core.Exhibits
{
	[UsedImplicitly]
	public sealed class YichuiPiao : Exhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.YichuiPiaoFlag + 1;
			gameRun.YichuiPiaoFlag = num;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.YichuiPiaoFlag - 1;
			gameRun.YichuiPiaoFlag = num;
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
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
