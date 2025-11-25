using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.Interactions
{
	public class TransformCardInteraction : Interaction
	{
		public IReadOnlyList<Card> PendingCards { get; }
		public Card TransformCard { get; set; }
		public Card SelectedCard { get; set; }
		public TransformCardInteraction(IEnumerable<Card> cards)
		{
			this.PendingCards = new List<Card>(cards).AsReadOnly();
		}
	}
}
