using System;
using LBoL.Core.Cards;
using LBoL.Core.Exhibits;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200019A RID: 410
	public sealed class RemoveCardAction : SimpleEventBattleAction<CardEventArgs>
	{
		// Token: 0x06000F09 RID: 3849 RVA: 0x00028993 File Offset: 0x00026B93
		public RemoveCardAction(Card card)
		{
			base.Args = new CardEventArgs
			{
				Card = card
			};
		}

		// Token: 0x06000F0A RID: 3850 RVA: 0x000289AD File Offset: 0x00026BAD
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.CardRemoving);
		}

		// Token: 0x06000F0B RID: 3851 RVA: 0x000289C0 File Offset: 0x00026BC0
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

		// Token: 0x06000F0C RID: 3852 RVA: 0x00028A49 File Offset: 0x00026C49
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.CardRemoved);
		}
	}
}
