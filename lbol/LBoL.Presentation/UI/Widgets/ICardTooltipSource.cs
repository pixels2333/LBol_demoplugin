using System;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public interface ICardTooltipSource
	{
		Card Card { get; }
		RectTransform RectTransform { get; }
		TooltipPosition[] TooltipPositions { get; }
	}
}
