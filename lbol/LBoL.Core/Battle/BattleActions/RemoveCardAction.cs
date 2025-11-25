using System;
using LBoL.Core.Cards;
using LBoL.Core.Exhibits;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class RemoveCardAction : SimpleEventBattleAction<CardEventArgs>
	{
		public RemoveCardAction(Card card)
		{
			base.Args = new CardEventArgs
			{
				Card = card
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.CardRemoving);
		}
		protected override void MainPhase()
		{
			base.Battle.RemoveCard(base.Args.Card);
			if (base.Battle.Player.IsInTurn && base.Battle.GameRun.YichuiPiaoFlag > 0 && base.Battle.HandZone.Count == 0)
			{
				YichuiPiao exhibit = base.Battle.Player.GetExhibit<YichuiPiao>();
				exhibit.NotifyActivating();
				base.Battle.React(new DrawCardsToSpecificAction(1), exhibit, ActionCause.Exhibit);
			}
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.CardRemoved);
		}
	}
}
