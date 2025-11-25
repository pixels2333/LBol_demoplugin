using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Mengriji : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed));
		}
		private void OnCardUsed(CardUsingEventArgs args)
		{
			if (args.Card.CardType == CardType.Defense)
			{
				base.Active = false;
			}
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Active)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			base.Active = true;
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Active = false;
		}
	}
}
