using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Adventures.Shared23
{
	// Token: 0x02000515 RID: 1301
	[AdventureInfo(WeighterType = typeof(KogasaSpook.KogasaSpookWeighter))]
	public sealed class KogasaSpook : Adventure
	{
		// Token: 0x06001110 RID: 4368 RVA: 0x0001ED5F File Offset: 0x0001CF5F
		[RuntimeCommand("kogasaUpgrade", "")]
		[UsedImplicitly]
		public IEnumerator KogasaUpgrade(string description)
		{
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.GameRun.BaseDeckWithoutUnremovable, (Card c) => c.CanUpgrade));
			int num = Math.Min(list.Count, 2);
			if (num == 0)
			{
				yield break;
			}
			SelectCardInteraction interaction = new SelectCardInteraction(num, num, list, SelectedCardHandling.DoNothing)
			{
				CanCancel = false,
				Description = description
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
			base.GameRun.UpgradeDeckCards(selectedCards, true);
			yield break;
		}

		// Token: 0x02000A6D RID: 2669
		private class KogasaSpookWeighter : IAdventureWeighter
		{
			// Token: 0x06003758 RID: 14168 RVA: 0x00086697 File Offset: 0x00084897
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((Enumerable.Count<Card>(gameRun.BaseDeck, (Card c) => c.CanUpgrade) > 1) ? 1 : 0);
			}
		}
	}
}
