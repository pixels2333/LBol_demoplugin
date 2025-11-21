using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x0200011A RID: 282
	[UsedImplicitly]
	public sealed class StartEthereal : JadeBox
	{
		// Token: 0x060003E6 RID: 998 RVA: 0x0000AD5F File Offset: 0x00008F5F
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x0000AD84 File Offset: 0x00008F84
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
