using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
using UnityEngine;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200013D RID: 317
	[UsedImplicitly]
	public sealed class ShijieSuipian : ShiningExhibit
	{
		// Token: 0x0600045B RID: 1115 RVA: 0x0000BA05 File Offset: 0x00009C05
		protected override IEnumerator SpecialGain(PlayerUnit player)
		{
			this.OnGain(player);
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.GameRun.BaseDeckWithoutUnremovable, (Card c) => c.CardType != CardType.Misfortune));
			int num = Math.Min(list.Count, base.Value1);
			if (num == 0)
			{
				yield break;
			}
			SelectCardInteraction interaction = new SelectCardInteraction(0, num, list, SelectedCardHandling.DoNothing)
			{
				Source = this,
				CanCancel = false
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
			if (selectedCards.Count == 0)
			{
				yield break;
			}
			List<Card> list2 = new List<Card>();
			list2.Add(base.GameRun.RollTransformCard(base.GameRun.GameRunEventRng, new CardWeightTable(RarityWeightTable.OnlyRare, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, null));
			List<Card> list3 = list2;
			if (selectedCards.Count > 1)
			{
				list3.Add(base.GameRun.RollTransformCard(base.GameRun.GameRunEventRng, new CardWeightTable(RarityWeightTable.OnlyUncommon, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), false, false, null));
			}
			if (selectedCards.Count > 2)
			{
				Debug.LogError("目前世界碎片只支持变化2张的情况");
			}
			base.GameRun.RemoveDeckCards(selectedCards, false);
			base.GameRun.AddDeckCards(list3, true, null);
			yield break;
		}
	}
}
