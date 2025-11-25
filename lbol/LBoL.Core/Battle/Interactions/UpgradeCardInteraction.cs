using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.Interactions
{
	public class UpgradeCardInteraction : Interaction
	{
		public IReadOnlyList<Card> PendingCards { get; }
		public Card SelectedCard { get; set; }
		public UpgradeCardInteraction(IEnumerable<Card> cards)
		{
			this.PendingCards = new List<Card>(cards).AsReadOnly();
		}
	}
}
