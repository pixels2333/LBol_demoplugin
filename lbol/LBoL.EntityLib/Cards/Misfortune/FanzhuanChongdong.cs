using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Misfortune
{
	[UsedImplicitly]
	public sealed class FanzhuanChongdong : Card
	{
		private bool LifeGained { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new GameEventHandler<UnitEventArgs>(this.OnTurnStarting), (GameEventPriority)0);
			base.HandleBattleEvent<HealEventArgs>(base.Battle.Player.HealingReceived, new GameEventHandler<HealEventArgs>(this.OnHealingReceived), (GameEventPriority)0);
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}
		private void OnTurnStarting(UnitEventArgs args)
		{
			this.LifeGained = false;
		}
		private void OnHealingReceived(HealEventArgs args)
		{
			this.LifeGained = true;
		}
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (!base.Battle.BattleShouldEnd && base.Zone == CardZone.Hand && !this.LifeGained)
			{
				base.NotifyActivating();
				yield return base.LoseLifeAction(base.Value1);
			}
			yield break;
		}
	}
}
