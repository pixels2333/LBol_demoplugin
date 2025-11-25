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
	[AdventureInfo(WeighterType = typeof(KogasaSpook.KogasaSpookWeighter))]
	public sealed class KogasaSpook : Adventure
	{
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
		private class KogasaSpookWeighter : IAdventureWeighter
		{
			public float WeightFor(Type type, GameRunController gameRun)
			{
				return (float)((Enumerable.Count<Card>(gameRun.BaseDeck, (Card c) => c.CanUpgrade) > 1) ? 1 : 0);
			}
		}
	}
}
