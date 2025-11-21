using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200004D RID: 77
	public class CommonToggleWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		// Token: 0x0600049F RID: 1183 RVA: 0x0001311E File Offset: 0x0001131E
		private void Awake()
		{
			this.Initialize();
		}

		// Token: 0x060004A0 RID: 1184 RVA: 0x00013128 File Offset: 0x00011328
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

		// Token: 0x060004A1 RID: 1185 RVA: 0x000131DC File Offset: 0x000113DC
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

		// Token: 0x060004A2 RID: 1186 RVA: 0x00013231 File Offset: 0x00011431
		public void OnPointerExit(PointerEventData eventData)
		{
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x00013233 File Offset: 0x00011433
		public void OnPointerClick(PointerEventData eventData)
		{
			if (this.lockInteractable && eventData.button == PointerEventData.InputButton.Left)
			{
				AudioManager.Button(this.toggle.isOn ? 0 : 1);
			}
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x0001325B File Offset: 0x0001145B
		public void SetLock(bool interactable)
		{
			this.lockInteractable = interactable;
			this.backImage.sprite = (interactable ? this.originalSprite : this.lockSprite);
		}

		// Token: 0x0400026E RID: 622
		public Toggle toggle;

		// Token: 0x0400026F RID: 623
		[SerializeField]
		private Image backImage;

		// Token: 0x04000270 RID: 624
		[SerializeField]
		private Sprite lockSprite;

		// Token: 0x04000271 RID: 625
		[SerializeField]
		private Sprite originalSprite;

		// Token: 0x04000272 RID: 626
		[SerializeField]
		private bool lockInteractable = true;

		// Token: 0x04000273 RID: 627
		[SerializeField]
		private bool hideBackImage;
	}
}
