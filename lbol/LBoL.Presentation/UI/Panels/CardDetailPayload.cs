using System;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.Presentation.UI.Panels
{
	public class CardDetailPayload
	{
		public CardDetailPayload(RectTransform rect, Card card, bool preventRightClickHide = false)
		{
			this.Rect = rect;
			this.Card = card;
			this.PreventRightClickHide = preventRightClickHide;
		}
		public RectTransform Rect;
		public Card Card;
		public bool PreventRightClickHide;
	}
}
