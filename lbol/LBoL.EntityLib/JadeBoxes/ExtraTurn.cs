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
	[UsedImplicitly]
	public sealed class ExtraTurn : JadeBox
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
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
