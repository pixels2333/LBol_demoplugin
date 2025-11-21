using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000048 RID: 72
	public class CharacterButtonWidget : CommonButtonWidget
	{
		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x0600047F RID: 1151 RVA: 0x00012978 File Offset: 0x00010B78
		// (set) Token: 0x06000480 RID: 1152 RVA: 0x00012980 File Offset: 0x00010B80
		public bool Interactable
		{
			get
			{
				return this._interactable;
			}
			set
			{
				this._interactable = value;
				base.GetComponent<Button>().interactable = value;
				base.GetComponent<Image>().material = (value ? null : this.grayMaterial);
			}
		}

		// Token: 0x06000481 RID: 1153 RVA: 0x000129AC File Offset: 0x00010BAC
		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (!this._interactable)
			{
				return;
			}
			this.outer.gameObject.SetActive(true);
		}

		// Token: 0x06000482 RID: 1154 RVA: 0x000129C8 File Offset: 0x00010BC8
		public override void OnPointerExit(PointerEventData eventData)
		{
			if (!this._interactable)
			{
				return;
			}
			this.outer.gameObject.SetActive(false);
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x000129E4 File Offset: 0x00010BE4
		private void OnDisable()
		{
			this.outer.gameObject.SetActive(false);
		}

		// Token: 0x04000251 RID: 593
		[SerializeField]
		private Transform outer;

		// Token: 0x04000252 RID: 594
		[SerializeField]
		private Material grayMaterial;

		// Token: 0x04000253 RID: 595
		private bool _interactable = true;
	}
}
