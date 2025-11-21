using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Sakuya;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x02000111 RID: 273
	[UsedImplicitly]
	public sealed class ExtraTurn : JadeBox
	{
		// Token: 0x060003BC RID: 956 RVA: 0x0000A64C File Offset: 0x0000884C
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x060003BD RID: 957 RVA: 0x0000A670 File Offset: 0x00008870
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			int turnCounter = base.Battle.Player.TurnCounter;
			if (turnCounter > 1 && (turnCounter - 1) % base.Value1 == 0)
			{
				ExtraTurnEveryone extraTurnEveryone = Library.CreateCard<ExtraTurnEveryone>();
				extraTurnEveryone.SetBaseCost(ManaGroup.Empty);
				yield return new AddCardsToHandAction(new Card[] { extraTurnEveryone });
			}
			yield break;
		}
	}
}
