using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class CommonToggleWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		private void Awake()
		{
			this.Initialize();
		}
		private void Initialize()
		{
			if (this.originalSprite == null && this.backImage != null)
			{
				this.originalSprite = this.backImage.sprite;
			}
			if (this.toggle == null)
			{
				this.toggle = base.GetComponent<Toggle>();
			}
			if (this.hideBackImage && this.toggle != null && this.backImage != null)
			{
				this.backImage.gameObject.SetActive(!this.toggle.isOn);
				this.toggle.onValueChanged.AddListener(delegate(bool on)
				{
					this.backImage.gameObject.SetActive(!on);
				});
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (base.gameObject.activeInHierarchy)
			{
				if (this.toggle == null)
				{
					this.toggle = base.GetComponent<Toggle>();
				}
				if (this.toggle != null && this.toggle.interactable)
				{
					AudioManager.Button(2);
				}
			}
		}
		public void OnPointerExit(PointerEventData eventData)
		{
		}
		public void OnPointerClick(PointerEventData eventData)
		{
			if (this.lockInteractable && eventData.button == PointerEventData.InputButton.Left)
			{
				AudioManager.Button(this.toggle.isOn ? 0 : 1);
			}
		}
		public void SetLock(bool interactable)
		{
			this.lockInteractable = interactable;
			this.backImage.sprite = (interactable ? this.originalSprite : this.lockSprite);
		}
		public Toggle toggle;
		[SerializeField]
		private Image backImage;
		[SerializeField]
		private Sprite lockSprite;
		[SerializeField]
		private Sprite originalSprite;
		[SerializeField]
		private bool lockInteractable = true;
		[SerializeField]
		private bool hideBackImage;
	}
}
