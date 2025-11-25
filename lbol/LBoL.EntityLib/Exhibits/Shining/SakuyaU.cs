using System;
using JetBrains.Annotations;
using LBoL.Core;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class SakuyaU : ShiningExhibit
	{
		protected override void OnEnterBattle()
		{
			base.Counter = base.Value2;
			base.Battle.DrawCardCount += base.Value1;
			base.HandleBattleEvent<UnitEventArgs>(base.Owner.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarting));
			base.HandleBattleEvent<UnitEventArgs>(base.Owner.TurnStarted, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private void OnPlayerTurnStarting(UnitEventArgs args)
		{
			if (base.Counter == 0 && base.Battle.Player.IsExtraTurn)
			{
				base.Battle.DrawCardCount += base.Value1;
			}
		}
		private void OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Counter > 0 && !base.Battle.Player.IsExtraTurn)
			{
				int num = base.Counter - 1;
				base.Counter = num;
				if (base.Counter == 0)
				{
					base.Battle.DrawCardCount -= base.Value1;
					return;
				}
			}
			else if (base.Counter == 0 && base.Battle.Player.IsExtraTurn)
			{
				base.Battle.DrawCardCount -= base.Value1;
			}
		}
		protected override void OnLeaveBattle()
		{
			if (base.Counter > 0)
			{
				base.Counter = 0;
				base.Battle.DrawCardCount -= base.Value1;
			}
		}
	}
}
