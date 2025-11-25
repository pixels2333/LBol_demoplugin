using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class StartEthereal : JadeBox
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private void OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				foreach (Card card in base.Battle.HandZone)
				{
					card.IsEthereal = true;
				}
			}
		}
	}
}
