using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.InputSystemExtend
{
	// Token: 0x020000ED RID: 237
	public class GamepadScrollRectItem : GamepadBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		// Token: 0x06000DB4 RID: 3508 RVA: 0x00041F63 File Offset: 0x00040163
		public void OnDeselect(BaseEventData eventData)
		{
			this.scrollRect == null;
		}

		// Token: 0x06000DB5 RID: 3509 RVA: 0x00041F74 File Offset: 0x00040174
		public void OnSelect(BaseEventData eventData)
		{
			if (this.scrollRect == null)
			{
				ScrollRect componentInParent = base.GetComponentInParent<ScrollRect>();
				if (componentInParent == null)
				{
					Debug.Log("No scrollrect when trigger OnSelect in GamepadScrollRectItem.");
					return;
				}
				this.scrollRect = componentInParent;
			}
			RectTransform viewport = this.scrollRect.viewport;
			RectTransform content = this.scrollRect.content;
			Vector2 anchoredPositionIn = GamepadScrollRectItem.GetAnchoredPositionIn(this.rectTransform, viewport);
			float num = anchoredPositionIn.y + this.rectTransform.rect.height * 0.5f;
			float num2 = anchoredPositionIn.y - this.rectTransform.rect.height * 0.5f;
			float num3 = anchoredPositionIn.x - this.rectTransform.rect.width * 0.5f;
			float num4 = anchoredPositionIn.x + this.rectTransform.rect.width * 0.5f;
			float yMax = viewport.rect.yMax;
			float yMin = viewport.rect.yMin;
			float xMin = viewport.rect.xMin;
			float xMax = viewport.rect.xMax;
			Vector2 anchoredPosition = content.anchoredPosition;
			if (num > yMax)
			{
				float num5 = num - yMax + 50f;
				anchoredPosition.y -= num5;
				if (anchoredPosition.y < 0f)
				{
					anchoredPosition.y = 0f;
				}
			}
			else if (num2 < yMin)
			{
				float num6 = num2 - yMin - 50f;
				anchoredPosition.y -= num6;
				if (anchoredPosition.y > content.rect.height - viewport.rect.height)
				{
					anchoredPosition.y = content.rect.height - viewport.rect.height;
				}
			}
			if (num4 > xMax)
			{
				float num7 = num4 - xMax + 50f;
				anchoredPosition.x -= num7;
				if (anchoredPosition.x < 0f)
				{
					anchoredPosition.x = 0f;
				}
			}
			else if (num3 < xMin)
			{
				float num8 = num3 - xMin - 50f;
				anchoredPosition.x -= num8;
				if (anchoredPosition.x > content.rect.width - viewport.rect.width)
				{
					anchoredPosition.x = content.rect.width - viewport.rect.width;
				}
			}
			if (this.disableHorizontal)
			{
				anchoredPosition.x = content.anchoredPosition.x;
			}
			if (this.disableVertical)
			{
				anchoredPosition.y = content.anchoredPosition.y;
			}
			content.DOAnchorPos(anchoredPosition, 0.2f, false).SetEase(Ease.OutCubic);
		}

		// Token: 0x06000DB6 RID: 3510 RVA: 0x00042247 File Offset: 0x00040447
		private void Awake()
		{
			this.rectTransform = base.GetComponent<RectTransform>();
		}

		// Token: 0x06000DB7 RID: 3511 RVA: 0x00042258 File Offset: 0x00040458
		protected override void Start()
		{
			base.Start();
			if (this.scrollRect != null)
			{
				return;
			}
			ScrollRect componentInParent = base.GetComponentInParent<ScrollRect>();
			if (componentInParent == null)
			{
				base.enabled = false;
				return;
			}
			this.scrollRect = componentInParent;
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x0004229C File Offset: 0x0004049C
		public static Vector2 GetAnchoredPositionIn(RectTransform a, RectTransform b)
		{
			Vector3 position = a.position;
			return b.InverseTransformPoint(position);
		}

		// Token: 0x04000A50 RID: 2640
		private ScrollRect scrollRect;

		// Token: 0x04000A51 RID: 2641
		private RectTransform rectTransform;

		// Token: 0x04000A52 RID: 2642
		[SerializeField]
		private bool disableHorizontal;

		// Token: 0x04000A53 RID: 2643
		[SerializeField]
		private bool disableVertical;
	}
}
