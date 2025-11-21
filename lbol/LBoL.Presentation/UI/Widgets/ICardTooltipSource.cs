using System;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000045 RID: 69
	public interface ICardTooltipSource
	{
		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x06000474 RID: 1140
		Card Card { get; }

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x06000475 RID: 1141
		RectTransform RectTransform { get; }

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x06000476 RID: 1142
		TooltipPosition[] TooltipPositions { get; }
	}
}
