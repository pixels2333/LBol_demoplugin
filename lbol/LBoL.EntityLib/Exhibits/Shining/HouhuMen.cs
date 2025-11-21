using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200012B RID: 299
	[UsedImplicitly]
	public sealed class HouhuMen : ShiningExhibit
	{
		// Token: 0x0600041B RID: 1051 RVA: 0x0000B314 File Offset: 0x00009514
		protected override IEnumerator SpecialGain(PlayerUnit player)
		{
			base.OnGain(player);
			int num = Math.Min(Enumerable.Count<Card>(base.GameRun.BaseDeck, (Card c) => !c.Unremovable), base.Value1);
			if (num > 0)
			{
				SelectCardInteraction interaction = new SelectCardInteraction(num, num, base.GameRun.BaseDeckWithoutUnremovable, SelectedCardHandling.DoNothing)
				{
					Source = this,
					CanCancel = false
				};
				yield return base.GameRun.InteractionViewer.View(interaction);
				base.GameRun.RemoveDeckCards(interaction.SelectedCards, true);
				interaction = null;
			}
			yield break;
		}
	}
}
