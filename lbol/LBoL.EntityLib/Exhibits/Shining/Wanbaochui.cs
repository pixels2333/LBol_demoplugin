using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class Wanbaochui : ShiningExhibit
	{
		protected override void OnAdded(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.WanbaochuiFlag + 1;
			gameRun.WanbaochuiFlag = num;
		}
		protected override void OnRemoved(PlayerUnit player)
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.WanbaochuiFlag - 1;
			gameRun.WanbaochuiFlag = num;
		}
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsing, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsing));
		}
		private void OnCardUsing(CardUsingEventArgs args)
		{
			if (args.Card.IsBasic && args.ConsumingMana.Colorless > 0)
			{
				base.NotifyActivating();
			}
		}
	}
}
