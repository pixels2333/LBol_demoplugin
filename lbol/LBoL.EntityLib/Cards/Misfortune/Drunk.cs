using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Misfortune
{
	[UsedImplicitly]
	public sealed class Drunk : Card
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Count = base.Battle.TurnCardUsageHistory.Count;
			this.NotifyChanged();
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new GameEventHandler<CardUsingEventArgs>(this.OnCardUsed), (GameEventPriority)0);
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, new GameEventHandler<UnitEventArgs>(this.OnTurnEnded), (GameEventPriority)0);
		}
		private void OnCardUsed(CardUsingEventArgs args)
		{
			this.Count = base.Battle.TurnCardUsageHistory.Count;
			this.NotifyChanged();
		}
		private void OnTurnEnded(UnitEventArgs args)
		{
			this.Count = 0;
			this.NotifyChanged();
		}
		public override bool ShouldPreventOtherCardUsage(Card card)
		{
			return this.Count >= base.Value1;
		}
		public override string PreventCardUsageMessage
		{
			get
			{
				return "ErrorChat.CardDrunk".Localize(true);
			}
		}
		[UsedImplicitly]
		public int Count;
	}
}
