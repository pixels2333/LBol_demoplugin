using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public interface IMultiCardTooltipSource
	{
		IEnumerable<Card> Cards { get; }
		RectTransform RectTransform { get; }
		TooltipPosition[] TooltipPositions { get; }
	}
}
