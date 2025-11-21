using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000A3 RID: 163
	[DisallowMultipleComponent]
	public sealed class MultipleCardTooltip : MonoBehaviour, IMultiCardTooltipSource, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000893 RID: 2195 RVA: 0x00029D1C File Offset: 0x00027F1C
		public RectTransform RectTransform
		{
			get
			{
				return base.GetComponent<RectTransform>();
			}
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x06000894 RID: 2196 RVA: 0x00029D24 File Offset: 0x00027F24
		public TooltipPosition[] TooltipPositions
		{
			get
			{
				return MultipleCardTooltip.DefaultTooltipPositions;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x06000895 RID: 2197 RVA: 0x00029D2B File Offset: 0x00027F2B
		// (set) Token: 0x06000896 RID: 2198 RVA: 0x00029D33 File Offset: 0x00027F33
		public IEnumerable<Card> Cards { get; set; }

		// Token: 0x06000897 RID: 2199 RVA: 0x00029D3C File Offset: 0x00027F3C
		private void OnDisable()
		{
			TooltipsLayer.Hide(this._tooltipId);
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x00029D49 File Offset: 0x00027F49
		public void OnPointerEnter(PointerEventData eventData)
		{
			this._tooltipId = TooltipsLayer.ShowCardMultiple(this);
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x00029D57 File Offset: 0x00027F57
		public void OnPointerExit(PointerEventData eventData)
		{
			TooltipsLayer.Hide(this._tooltipId);
		}

		// Token: 0x0400062B RID: 1579
		private static readonly TooltipPosition[] DefaultTooltipPositions = new TooltipPosition[]
		{
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Left, TooltipAlignment.Min),
			new TooltipPosition(TooltipDirection.Right, TooltipAlignment.Max),
			new TooltipPosition(TooltipDirection.Top, TooltipAlignment.Center),
			new TooltipPosition(TooltipDirection.Bottom, TooltipAlignment.Center)
		};

		// Token: 0x0400062C RID: 1580
		private int _num;

		// Token: 0x0400062E RID: 1582
		private int _tooltipId;
	}
}
