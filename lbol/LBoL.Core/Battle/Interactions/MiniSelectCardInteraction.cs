using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.Interactions
{
	public class MiniSelectCardInteraction : Interaction
	{
		public Card SelectedCard { get; set; }
		public IReadOnlyList<Card> PendingCards { get; }
		public bool HasSlideInAnimation { get; }
		public bool IsAddCardToDeck { get; }
		public bool CanSkip { get; }
		public MiniSelectCardInteraction(IEnumerable<Card> cards, bool hasSlideInAnimation = false, bool isAddCardToDeck = false, bool canSkip = false)
		{
			this.PendingCards = new List<Card>(cards).AsReadOnly();
			this.HasSlideInAnimation = hasSlideInAnimation;
			this.IsAddCardToDeck = isAddCardToDeck;
			this.CanSkip = canSkip;
		}
	}
}
