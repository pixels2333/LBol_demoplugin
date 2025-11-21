using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B7 RID: 439
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(Zijing.ZijingWeighter))]
	public sealed class Zijing : Exhibit
	{
		// Token: 0x06000651 RID: 1617 RVA: 0x0000E9FB File Offset: 0x0000CBFB
		protected override IEnumerator SpecialGain(PlayerUnit player)
		{
			int num = base.Value1;
			IReadOnlyList<Card> baseDeck = base.GameRun.BaseDeck;
			if (num > baseDeck.Count)
			{
				Debug.LogError(string.Format("{0}: Cannot select {1} cards (has {2})", this.DebugName, num, baseDeck.Count));
				if (baseDeck.Count == 0)
				{
					yield break;
				}
				num = baseDeck.Count;
			}
			SelectCardInteraction interaction = new SelectCardInteraction(num, num, baseDeck, SelectedCardHandling.DoNothing)
			{
				Source = this,
				CanCancel = false
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			Card[] array = Enumerable.ToArray<Card>(Enumerable.Select<Card, Card>(interaction.SelectedCards, (Card card) => card.Clone(false)));
			base.GameRun.AddDeckCards(array, false, null);
			base.Blackout = true;
			yield break;
		}

		// Token: 0x02000672 RID: 1650
		private class ZijingWeighter : IExhibitWeighter
		{
			// Token: 0x06001AC5 RID: 6853 RVA: 0x000373EF File Offset: 0x000355EF
			public float WeightFor(Type type, GameRunController gameRun)
			{
				if (gameRun.CurrentStage.Level <= 1 && gameRun.CurrentStation.Level <= 5)
				{
					return 0f;
				}
				return 1f;
			}
		}
	}
}
