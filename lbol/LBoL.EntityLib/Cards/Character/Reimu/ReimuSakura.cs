using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class ReimuSakura : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new GameEventHandler<GameEventArgs>(this.OnBattleStarted), (GameEventPriority)0);
		}
		private void OnBattleStarted(GameEventArgs args)
		{
			if (base.Battle.CardExtraGrowAmount > 0)
			{
				base.DecreaseBaseCost(base.Mana * base.Battle.CardExtraGrowAmount);
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.HealAction(base.Value1);
			yield break;
		}
		public override IEnumerable<BattleAction> AfterUseAction()
		{
			base.DecreaseBaseCost(base.Mana);
			return base.AfterUseAction();
		}
	}
}
