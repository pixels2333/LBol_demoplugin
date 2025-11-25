using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LBoL.Presentation.UI.Panels
{
	[DisallowMultipleComponent]
	public sealed class MultipleCardTooltip : MonoBehaviour, IMultiCardTooltipSource, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public RectTransform RectTransform
		{
			get
			{
				return base.GetComponent<RectTransform>();
			}
		}
		public TooltipPosition[] TooltipPositions
		{
			get
			{
				return MultipleCardTooltip.DefaultTooltipPositions;
			}
		}
		public IEnumerable<Card> Cards { get; set; }
		private void OnDisable()
		{
			TooltipsLayer.Hide(this._tooltipId);
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			this._tooltipId = TooltipsLayer.ShowCardMultiple(this);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			TooltipsLayer.Hide(this._tooltipId);
		}
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Min),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center)
		};
		private int _num;
		private int _tooltipId;
	}
}
