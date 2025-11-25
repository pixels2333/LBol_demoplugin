using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Presentation.UI.Panels
{
	public class ShowCardsPayload
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public IReadOnlyList<Card> Cards { get; set; }
		public bool CanCancel { get; set; } = true;
		public InteractionType InteractionType { get; set; }
		public ShowCardZone CardZone { get; set; }
		public bool HideActualOrder { get; set; }
		public IReadOnlyList<Card> PayCards { get; set; }
		public int Money { get; set; }
		public int Price { get; set; }
	}
}
