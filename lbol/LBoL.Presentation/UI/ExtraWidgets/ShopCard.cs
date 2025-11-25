using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.ExtraWidgets
{
	public class ShopCard : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public ShopPanel ShopPanel { get; set; }
		public int Index { get; set; }
		public Card Card
		{
			get
			{
				return this.cardWidget.Card;
			}
		}
		private bool Active { get; set; }
		private bool CanAfford { get; set; }
		private bool Hovering { get; set; }
		private RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}
		public RectTransform CardRectTransform
		{
			get
			{
				return (RectTransform)this.cardWidget.transform;
			}
		}
		public void SetCard(Card cardContent, int cardPrice, bool canAfford, bool isDiscounted = false)
		{
			this.cardWidget.Card = cardContent;
			this.SetPrice(cardPrice, canAfford, isDiscounted);
			this.Active = true;
			this.discount.gameObject.SetActive(isDiscounted);
			this.content.gameObject.SetActive(true);
			this.content.localScale = new Vector3(1f, 1f, 1f);
			this.soldOut.gameObject.SetActive(false);
		}
		public void SetPrice(int cardPrice, bool canAfford, bool isDiscounted = false)
		{
			this.price.text = cardPrice.ToString();
			this.CanAfford = canAfford;
			if (canAfford && isDiscounted)
			{
				this.price.color = ShopPanel.Discount;
				return;
			}
			this.price.color = (this.CanAfford ? ShopPanel.EnoughColor : ShopPanel.NotEnoughColor);
		}
		public void ViewBought()
		{
			this.Active = false;
			this.content.gameObject.SetActive(false);
			this.soldOut.gameObject.SetActive(true);
		}
		private void Update()
		{
			if (this.tiltWhenHover && this.Hovering)
			{
				if (Mouse.current == null)
				{
					return;
				}
				float num;
				float num2;
				Mouse.current.position.ReadValue().Deconstruct(out num, out num2);
				float num3 = num;
				float num4 = num2;
				int width = Screen.width;
				if (Math.Abs(width - 3840) > 1)
				{
					this._screenScale = 3840f / (float)width;
				}
				else
				{
					this._screenScale = 1f;
				}
				this._pointerX = num3 * this._screenScale;
				this._pointerY = num4 * this._screenScale;
				Vector3 vector = CameraController.UiCamera.WorldToScreenPoint(this.RectTransform.position) * this._screenScale;
				Vector2 vector2 = new Vector2(this._pointerX - vector.x, this._pointerY - vector.y);
				Quaternion quaternion = Quaternion.AngleAxis(Mathf.Clamp01(vector2.magnitude / 546f) * 5f, new Vector3(vector2.y, -vector2.x));
				this.CardRectTransform.localRotation = quaternion * Quaternion.identity;
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (!this.Active)
			{
				return;
			}
			this.Hovering = true;
			base.transform.DOScale(1.2f, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			this.cardWidget.ShowTooltip();
			AudioManager.Card(0);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.Hovering = false;
			base.transform.DOScale(1f, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			this.cardWidget.HideTooltip();
			if (this.tiltWhenHover)
			{
				this.CardRectTransform.DOLocalRotateQuaternion(Quaternion.identity, 0.12f).SetUpdate(true).SetEase(Ease.OutCubic);
			}
		}
		public void OnPointerClick(PointerEventData eventData)
		{
			if (!this.Active || this.ShopPanel.LockedByInteractionMinimized)
			{
				return;
			}
			PointerEventData.InputButton button = eventData.button;
			if (button != PointerEventData.InputButton.Left)
			{
				if (button == PointerEventData.InputButton.Right)
				{
					AudioManager.Card(3);
					UiManager.GetPanel<CardDetailPanel>().Show(new CardDetailPayload(this.cardWidget.GetComponent<RectTransform>(), this.cardWidget.Card, false));
					return;
				}
			}
			else
			{
				if (this.CanAfford)
				{
					this.ShopPanel.BuyCard(this.Index);
					AudioManager.PlayUi("Bought", false);
					return;
				}
				this.ShopPanel.QuoteCantAfford();
				AudioManager.PlayUi("NoMoney", false);
			}
		}
		public CardWidget CloneCardWidget(Transform parent = null)
		{
			CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardWidget, base.transform);
			cardWidget.Card = this.Card;
			if (parent != null)
			{
				cardWidget.transform.SetParent(parent);
			}
			return cardWidget;
		}
		[SerializeField]
		private Transform content;
		[SerializeField]
		private Image soldOut;
		[SerializeField]
		private Image discount;
		[SerializeField]
		private CardWidget cardWidget;
		[SerializeField]
		private TextMeshProUGUI price;
		private const float NormalScale = 1f;
		private const float HoverScale = 1.2f;
		public bool tiltWhenHover = true;
		private const float MaxTiltAngle = 5f;
		private const float CardRadius = 455f;
		private float _pointerX;
		private float _pointerY;
		private float _screenScale = 1f;
	}
}
