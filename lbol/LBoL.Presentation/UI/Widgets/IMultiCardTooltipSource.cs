using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000046 RID: 70
	public interface IMultiCardTooltipSource
	{
		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x06000477 RID: 1143
		IEnumerable<Card> Cards { get; }

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000478 RID: 1144
		RectTransform RectTransform { get; }

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x06000479 RID: 1145
		TooltipPosition[] TooltipPositions { get; }
	}
}
