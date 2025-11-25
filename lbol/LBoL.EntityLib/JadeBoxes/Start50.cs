using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class Start50 : JadeBox
	{
		protected override void OnGain(GameRunController gameRun)
		{
			gameRun.RemoveGamerunInitialCards();
			for (int i = 0; i < base.Value1; i++)
			{
				Card[] array = gameRun.RollCards(gameRun.CardRng, new CardWeightTable(RarityWeightTable.EnemyCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), 1, false, false, null);
				gameRun.AddDeckCards(array, false, null);
			}
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			yield return ConvertManaAction.PhilosophyRandomMana(base.Battle.BattleMana, 1, base.GameRun.BattleRng);
			yield break;
		}
	}
}
