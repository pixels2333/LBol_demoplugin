using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.Interactions
{
	public class SelectHandInteraction : Interaction
	{
		public int Min { get; }
		public int Max { get; }
		public bool Sortable { get; set; }
		public IReadOnlyList<Card> PendingCards { get; }
		public SelectHandInteraction(int min, int max, IEnumerable<Card> cards)
		{
			this.Min = min;
			this.Max = max;
			this.Sortable = this.Max > 1;
			this.PendingCards = new List<Card>(cards).AsReadOnly();
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
					throw new InvalidOperationException(string.Format("Invalid {0} count = {1} for {2}", "value", value.Count, "SelectHandInteraction"));
				}
				this._selectedCards = value;
			}
		}
		private IReadOnlyList<Card> _selectedCards;
	}
}
