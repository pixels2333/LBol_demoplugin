using System;
using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class Bumengwang : Exhibit
	{
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
			base.GameRun.Heal(base.Value2, true, null);
			base.GameRun.GainPower(base.Value3, false);
			yield break;
		}
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
