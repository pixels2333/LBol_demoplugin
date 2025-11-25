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
	[UsedImplicitly]
	[ExhibitInfo(WeighterType = typeof(Zijing.ZijingWeighter))]
	public sealed class Zijing : Exhibit
	{
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
		private class ZijingWeighter : IExhibitWeighter
		{
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
