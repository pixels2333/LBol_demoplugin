using System;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200008B RID: 139
	public class CardDetailPayload
	{
		// Token: 0x06000713 RID: 1811 RVA: 0x00020881 File Offset: 0x0001EA81
		public CardDetailPayload(RectTransform rect, Card card, bool preventRightClickHide = false)
		{
			this.Rect = rect;
			this.Card = card;
			this.PreventRightClickHide = preventRightClickHide;
		}

		// Token: 0x04000490 RID: 1168
		public RectTransform Rect;

		// Token: 0x04000491 RID: 1169
		public Card Card;

		// Token: 0x04000492 RID: 1170
		public bool PreventRightClickHide;
	}
}
