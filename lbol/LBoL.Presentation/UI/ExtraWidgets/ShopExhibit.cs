using System;
using DG.Tweening;
using LBoL.Core;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	public class ShopExhibit : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public ShopPanel ShopPanel { get; set; }
		public int Index { get; set; }
		public Exhibit Exhibit
		{
			get
			{
				return this.exhibit.Exhibit;
			}
		}
		private void Awake()
		{
			this.exhibit.ExhibitClicked += delegate
			{
				if (!this._active || this.ShopPanel.LockedByInteractionMinimized)
				{
					return;
				}
				if (this._canAfford && this._canBuy)
				{
					this.ShopPanel.BuyExhibit(this.Index);
					AudioManager.PlayUi("Bought", false);
					return;
				}
				this.ShopPanel.QuoteCantAfford();
				AudioManager.PlayUi("NoMoney", false);
			};
		}
		public void SetExhibit(Exhibit exhibitContent, int exhibitPrice, bool canAfford)
		{
			this.exhibit.Exhibit = exhibitContent;
			this.SetPrice(exhibitPrice, canAfford);
			this._active = true;
			this._canBuy = !exhibitContent.Config.IsSentinel;
			this.root.gameObject.SetActive(true);
			this.bg.gameObject.SetActive(false);
			this.price.gameObject.SetActive(this._canBuy);
			this.exhibit.ShowCounter = false;
		}
		public void SetPrice(int exhibitPrice, bool canAfford)
		{
			this.price.text = exhibitPrice.ToString();
			this._canAfford = canAfford;
			this.price.color = (this._canAfford ? ShopPanel.EnoughColor : ShopPanel.NotEnoughColor);
		}
		public void Close()
		{
			this._active = false;
			this.root.gameObject.SetActive(false);
			this.bg.gameObject.SetActive(true);
			this.bg.DOFade(1f, 0.4f).From(0f, true, false);
			this.bg.transform.DOScale(1f, 0.4f).From(1.2f, true, false);
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this._active)
			{
				this.root.DOScale(1.2f, 0.1f);
			}
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			if (this._active)
			{
				this.root.DOScale(1f, 0.3f);
			}
		}
		[SerializeField]
		private Transform root;
		[SerializeField]
		private Image bg;
		[SerializeField]
		private ExhibitWidget exhibit;
		[SerializeField]
		private TextMeshProUGUI price;
		private bool _active;
		private bool _canAfford;
		private bool _canBuy;
	}
}
