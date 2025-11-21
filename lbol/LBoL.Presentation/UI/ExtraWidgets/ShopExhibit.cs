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
	// Token: 0x020000D2 RID: 210
	public class ShopExhibit : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x17000210 RID: 528
		// (get) Token: 0x06000CBD RID: 3261 RVA: 0x0003FCDF File Offset: 0x0003DEDF
		// (set) Token: 0x06000CBE RID: 3262 RVA: 0x0003FCE7 File Offset: 0x0003DEE7
		public ShopPanel ShopPanel { get; set; }

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06000CBF RID: 3263 RVA: 0x0003FCF0 File Offset: 0x0003DEF0
		// (set) Token: 0x06000CC0 RID: 3264 RVA: 0x0003FCF8 File Offset: 0x0003DEF8
		public int Index { get; set; }

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x06000CC1 RID: 3265 RVA: 0x0003FD01 File Offset: 0x0003DF01
		public Exhibit Exhibit
		{
			get
			{
				return this.exhibit.Exhibit;
			}
		}

		// Token: 0x06000CC2 RID: 3266 RVA: 0x0003FD0E File Offset: 0x0003DF0E
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

		// Token: 0x06000CC3 RID: 3267 RVA: 0x0003FD28 File Offset: 0x0003DF28
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

		// Token: 0x06000CC4 RID: 3268 RVA: 0x0003FDA8 File Offset: 0x0003DFA8
		public void SetPrice(int exhibitPrice, bool canAfford)
		{
			this.price.text = exhibitPrice.ToString();
			this._canAfford = canAfford;
			this.price.color = (this._canAfford ? ShopPanel.EnoughColor : ShopPanel.NotEnoughColor);
		}

		// Token: 0x06000CC5 RID: 3269 RVA: 0x0003FDE4 File Offset: 0x0003DFE4
		public void Close()
		{
			this._active = false;
			this.root.gameObject.SetActive(false);
			this.bg.gameObject.SetActive(true);
			this.bg.DOFade(1f, 0.4f).From(0f, true, false);
			this.bg.transform.DOScale(1f, 0.4f).From(1.2f, true, false);
		}

		// Token: 0x06000CC6 RID: 3270 RVA: 0x0003FE63 File Offset: 0x0003E063
		public void OnPointerEnter(PointerEventData eventData)
		{
			if (this._active)
			{
				this.root.DOScale(1.2f, 0.1f);
			}
		}

		// Token: 0x06000CC7 RID: 3271 RVA: 0x0003FE83 File Offset: 0x0003E083
		public void OnPointerExit(PointerEventData eventData)
		{
			if (this._active)
			{
				this.root.DOScale(1f, 0.3f);
			}
		}

		// Token: 0x040009BB RID: 2491
		[SerializeField]
		private Transform root;

		// Token: 0x040009BC RID: 2492
		[SerializeField]
		private Image bg;

		// Token: 0x040009BD RID: 2493
		[SerializeField]
		private ExhibitWidget exhibit;

		// Token: 0x040009BE RID: 2494
		[SerializeField]
		private TextMeshProUGUI price;

		// Token: 0x040009C1 RID: 2497
		private bool _active;

		// Token: 0x040009C2 RID: 2498
		private bool _canAfford;

		// Token: 0x040009C3 RID: 2499
		private bool _canBuy;
	}
}
