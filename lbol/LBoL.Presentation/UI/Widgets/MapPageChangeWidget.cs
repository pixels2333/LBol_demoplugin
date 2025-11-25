using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class MapPageChangeWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		private void Awake()
		{
			this.signal.transform.localScale = Vector3.zero;
		}
		public void Animate(bool active)
		{
			this.OnPointerExit(null);
			if (active && !this.isActive)
			{
				this.signal.transform.DOScale(1f, 0.2f).From(0f, true, false);
			}
			if (!active && this.isActive)
			{
				this.signal.transform.DOScale(0f, 0.2f).From(1f, true, false);
			}
			this.isActive = active;
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this.isActive)
			{
				return;
			}
			this.isEnter = true;
			this.signal.transform.DOScale(0.6f, 0.1f).From(0f, true, false);
			this.signal.DOFade(0.5f, 0f);
			this.border.transform.DOScale(1.2f, 0.1f).From(1f, true, false);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			if (!this.isEnter)
			{
				return;
			}
			if (this.isActive)
			{
				return;
			}
			this.isEnter = false;
			this.signal.transform.DOScale(0f, 0.1f).From(0.6f, true, false);
			this.signal.DOFade(1f, 0f);
			this.border.transform.DOScale(1f, 0.1f).From(1.2f, true, false);
		}
		[SerializeField]
		private Image signal;
		[SerializeField]
		private Image border;
		[SerializeField]
		public Button button;
		[SerializeField]
		private bool isActive;
		[SerializeField]
		private bool isEnter;
	}
}
