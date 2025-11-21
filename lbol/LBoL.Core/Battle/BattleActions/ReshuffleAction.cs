using System;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200019F RID: 415
	public sealed class ReshuffleAction : SimpleEventBattleAction<GameEventArgs>
	{
		// Token: 0x06000F18 RID: 3864 RVA: 0x00028BD2 File Offset: 0x00026DD2
		public ReshuffleAction()
		{
			base.Args = new GameEventArgs
			{
				CanCancel = false
			};
		}

		// Token: 0x06000F19 RID: 3865 RVA: 0x00028BEC File Offset: 0x00026DEC
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.Reshuffling);
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x00028C00 File Offset: 0x00026E00
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

		// Token: 0x06000F1B RID: 3867 RVA: 0x00028CA8 File Offset: 0x00026EA8
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.Reshuffled);
		}
	}
}
