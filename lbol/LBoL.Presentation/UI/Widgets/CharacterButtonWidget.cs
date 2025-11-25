using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class CharacterButtonWidget : CommonButtonWidget
	{
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
		public override void OnPointerEnter(PointerEventData eventData)
		{
			if (!this._interactable)
			{
				return;
			}
			this.outer.gameObject.SetActive(true);
		}
		public override void OnPointerExit(PointerEventData eventData)
		{
			if (!this._interactable)
			{
				return;
			}
			this.outer.gameObject.SetActive(false);
		}
		private void OnDisable()
		{
			this.outer.gameObject.SetActive(false);
		}
		[SerializeField]
		private Transform outer;
		[SerializeField]
		private Material grayMaterial;
		private bool _interactable = true;
	}
}
