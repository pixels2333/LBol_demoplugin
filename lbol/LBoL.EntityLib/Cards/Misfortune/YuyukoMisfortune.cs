using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Misfortune
{
	[UsedImplicitly]
	public sealed class YuyukoMisfortune : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Battle.Player.IsInTurn && base.Zone == CardZone.Hand && args.Card != this)
			{
				base.NotifyActivating();
				yield return base.LoseLifeAction(base.Value1);
			}
			yield break;
		}
	}
}
