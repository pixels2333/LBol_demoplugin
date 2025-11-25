using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.InputSystemExtend
{
	public class GamepadScrollRectItem : GamepadBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		public void OnDeselect(BaseEventData eventData)
		{
			this.scrollRect == null;
		}
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
		private void Awake()
		{
			this.rectTransform = base.GetComponent<RectTransform>();
		}
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
		public static Vector2 GetAnchoredPositionIn(RectTransform a, RectTransform b)
		{
			Vector3 position = a.position;
			return b.InverseTransformPoint(position);
		}
		private ScrollRect scrollRect;
		private RectTransform rectTransform;
		[SerializeField]
		private bool disableHorizontal;
		[SerializeField]
		private bool disableVertical;
	}
}
