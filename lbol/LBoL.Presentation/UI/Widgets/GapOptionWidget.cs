using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core.GapOptions;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class GapOptionWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		public GapOptionsPanel Parent { get; set; }
		public bool Active { get; set; } = true;
		private void Awake()
		{
			this.optionTip.gameObject.SetActive(false);
		}
		public void OnLocalizeChanged()
		{
			if (this._option != null)
			{
				this.optionName.SetText(this._option.Name, true);
				this.optionTipText.SetText(this._option.Description, true);
			}
		}
		public void SetOption(GapOption option, Sprite sprite)
		{
			this._option = option;
			this.optionName.text = option.Name;
			this.image.sprite = sprite;
			this.optionTipText.text = option.Description;
		}
		public void OnPointerClick(PointerEventData eventData)
		{
			if (this.Active)
			{
				this.Parent.OptionClicked(this._option);
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.root.DOScale(1.1f, 0.2f).SetLink(base.gameObject);
			this.Parent.StartHoverOption(this._option);
			this.optionTip.gameObject.SetActive(true);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.root.DOScale(1f, 0.2f).SetLink(base.gameObject);
			this.Parent.EndHoverOption();
			this.optionTip.gameObject.SetActive(false);
		}
		public void OnDisable()
		{
			this.optionTip.gameObject.SetActive(false);
		}
		[SerializeField]
		private Transform root;
		[SerializeField]
		private Image image;
		[SerializeField]
		private TextMeshProUGUI optionName;
		[SerializeField]
		private Transform optionTip;
		[SerializeField]
		private TextMeshProUGUI optionTipText;
		private GapOption _option;
	}
}
