using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000127 RID: 295
	[UsedImplicitly]
	public sealed class FengshouGuolan : ShiningExhibit
	{
		// Token: 0x0600040B RID: 1035 RVA: 0x0000B113 File Offset: 0x00009313
		protected override void OnGain(PlayerUnit player)
		{
			base.OnGain(player);
			base.GameRun.GainMaxHp(base.Value1, true, true);
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x0000B12F File Offset: 0x0000932F
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsAdded, new GameEventHandler<CardsEventArgs>(this.OnDeckCardAdded));
		}

		// Token: 0x0600040D RID: 1037 RVA: 0x0000B14E File Offset: 0x0000934E
		private void OnDeckCardAdded(CardsEventArgs args)
		{
			base.NotifyActivating();
			base.GameRun.GainMaxHp(base.Value2 * args.Cards.Length, true, true);
		}
	}
}
