using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Shining
{
	[UsedImplicitly]
	public sealed class FengshouGuolan : ShiningExhibit
	{
		protected override void OnGain(PlayerUnit player)
		{
			base.OnGain(player);
			base.GameRun.GainMaxHp(base.Value1, true, true);
		}
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsAdded, new GameEventHandler<CardsEventArgs>(this.OnDeckCardAdded));
		}
		private void OnDeckCardAdded(CardsEventArgs args)
		{
			base.NotifyActivating();
			base.GameRun.GainMaxHp(base.Value2 * args.Cards.Length, true, true);
		}
	}
}
