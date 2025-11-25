using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.Interactions
{
	public class SelectCardInteraction : Interaction
	{
		public int Min { get; }
		public int Max { get; }
		public bool Sortable { get; set; }
		public IReadOnlyList<Card> PendingCards { get; }
		public SelectedCardHandling Handling { get; }
		public SelectCardInteraction(int min, int max, IEnumerable<Card> cards, SelectedCardHandling handling = SelectedCardHandling.DoNothing)
		{
			this.Min = min;
			this.Max = max;
			this.Sortable = this.Max > 1;
			this.PendingCards = new List<Card>(cards).AsReadOnly();
			this.Handling = handling;
		}
		public SelectCardInteraction(int min, int max, bool sortable, IEnumerable<Card> cards, SelectedCardHandling handling = SelectedCardHandling.DoNothing)
		{
			this.Min = min;
			this.Max = max;
			this.Sortable = sortable;
			this.PendingCards = new List<Card>(cards).AsReadOnly();
			this.Handling = handling;
		}
		public IReadOnlyList<Card> SelectedCards
		{
			get
			{
				return this._selectedCards;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (value.Count < this.Min || value.Count > this.Max)
				{
					throw new InvalidOperationException(string.Format("Invalid {0} count = {1} for {2}", "value", value.Count, "SelectCardInteraction"));
				}
				this._selectedCards = value;
			}
		}
		private IReadOnlyList<Card> _selectedCards;
	}
}
