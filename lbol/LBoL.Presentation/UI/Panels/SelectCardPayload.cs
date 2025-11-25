using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Presentation.UI.Panels
{
	public class SelectCardPayload
	{
		public string Name { get; set; }
		public IEnumerable<Card> Cards { get; set; }
		public int Min { get; set; }
		public int Max { get; set; }
		public bool Sortable { get; set; }
		public bool CanCancel { get; set; }
		public bool CanSkip { get; set; }
		public bool IsAddCardToDeck { get; set; }
	}
}
