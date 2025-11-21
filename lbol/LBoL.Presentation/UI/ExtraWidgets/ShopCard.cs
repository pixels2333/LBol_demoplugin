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
	// Token: 0x020000D1 RID: 209
	public class ShopCard : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		// Token: 0x17000208 RID: 520
		// (get) Token: 0x06000CA7 RID: 3239 RVA: 0x0003F874 File Offset: 0x0003DA74
		// (set) Token: 0x06000CA8 RID: 3240 RVA: 0x0003F87C File Offset: 0x0003DA7C
		public ShopPanel ShopPanel { get; set; }

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x06000CA9 RID: 3241 RVA: 0x0003F885 File Offset: 0x0003DA85
		// (set) Token: 0x06000CAA RID: 3242 RVA: 0x0003F88D File Offset: 0x0003DA8D
		public int Index { get; set; }

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06000CAB RID: 3243 RVA: 0x0003F896 File Offset: 0x0003DA96
		public Card Card
		{
			get
			{
				return this.cardWidget.Card;
			}
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000CAC RID: 3244 RVA: 0x0003F8A3 File Offset: 0x0003DAA3
		// (set) Token: 0x06000CAD RID: 3245 RVA: 0x0003F8AB File Offset: 0x0003DAAB
		private bool Active { get; set; }

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000CAE RID: 3246 RVA: 0x0003F8B4 File Offset: 0x0003DAB4
		// (set) Token: 0x06000CAF RID: 3247 RVA: 0x0003F8BC File Offset: 0x0003DABC
		private bool CanAfford { get; set; }

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x06000CB0 RID: 3248 RVA: 0x0003F8C5 File Offset: 0x0003DAC5
		// (set) Token: 0x06000CB1 RID: 3249 RVA: 0x0003F8CD File Offset: 0x0003DACD
		private bool Hovering { get; set; }

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x06000CB2 RID: 3250 RVA: 0x0003F8D6 File Offset: 0x0003DAD6
		private RectTransform RectTransform
		{
			get
			{
				return (RectTransform)base.transform;
			}
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x06000CB3 RID: 3251 RVA: 0x0003F8E3 File Offset: 0x0003DAE3
		public RectTransform CardRectTransform
		{
			get
			{
				return (RectTransform)this.cardWidget.transform;
			}
		}

		// Token: 0x06000CB4 RID: 3252 RVA: 0x0003F8F8 File Offset: 0x0003DAF8
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

		// Token: 0x06000CB5 RID: 3253 RVA: 0x0003F978 File Offset: 0x0003DB78
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

		// Token: 0x06000CB6 RID: 3254 RVA: 0x0003F9D3 File Offset: 0x0003DBD3
		public void ViewBought()
		{
			this.Active = false;
			this.content.gameObject.SetActive(false);
			this.soldOut.gameObject.SetActive(true);
		}

		// Token: 0x06000CB7 RID: 3255 RVA: 0x0003FA00 File Offset: 0x0003DC00
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

		// Token: 0x06000CB8 RID: 3256 RVA: 0x0003FB24 File Offset: 0x0003DD24
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

		// Token: 0x06000CB9 RID: 3257 RVA: 0x0003FB78 File Offset: 0x0003DD78
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

		// Token: 0x06000CBA RID: 3258 RVA: 0x0003FBE8 File Offset: 0x0003DDE8
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

		// Token: 0x06000CBB RID: 3259 RVA: 0x0003FC84 File Offset: 0x0003DE84
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

		// Token: 0x040009A9 RID: 2473
		[SerializeField]
		private Transform content;

		// Token: 0x040009AA RID: 2474
		[SerializeField]
		private Image soldOut;

		// Token: 0x040009AB RID: 2475
		[SerializeField]
		private Image discount;

		// Token: 0x040009AC RID: 2476
		[SerializeField]
		private CardWidget cardWidget;

		// Token: 0x040009AD RID: 2477
		[SerializeField]
		private TextMeshProUGUI price;

		// Token: 0x040009B2 RID: 2482
		private const float NormalScale = 1f;

		// Token: 0x040009B3 RID: 2483
		private const float HoverScale = 1.2f;

		// Token: 0x040009B4 RID: 2484
		public bool tiltWhenHover = true;

		// Token: 0x040009B6 RID: 2486
		private const float MaxTiltAngle = 5f;

		// Token: 0x040009B7 RID: 2487
		private const float CardRadius = 455f;

		// Token: 0x040009B8 RID: 2488
		private float _pointerX;

		// Token: 0x040009B9 RID: 2489
		private float _pointerY;

		// Token: 0x040009BA RID: 2490
		private float _screenScale = 1f;
	}
}
