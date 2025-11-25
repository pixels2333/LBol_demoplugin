using System;
using System.Linq;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class ReshuffleAction : SimpleEventBattleAction<GameEventArgs>
	{
		public ReshuffleAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.Reshuffling);
		}
		protected override void MainPhase()
		{
			base.Battle.Reshuffle();
			if (Enumerable.Any<Card>(base.Battle.DrawZone, (Card card) => card.IsDreamCard))
			{
				base.React(new Reactor(new DreamCardsToHandAction()), null, default(ActionCause?));
				return;
			}
			foreach (Card card2 in base.Battle.EnumerateAllCards())
			{
				card2.IsDreamCard = false;
			}
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.Reshuffled);
		}
	}
}
