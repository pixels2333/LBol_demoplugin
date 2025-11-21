using System;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LBoL.Presentation.UI.ExtraWidgets
{
	// Token: 0x020000D0 RID: 208
	[RequireComponent(typeof(CardWidget))]
	public class SelectCardWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		// Token: 0x17000204 RID: 516
		// (get) Token: 0x06000C98 RID: 3224 RVA: 0x0003F721 File Offset: 0x0003D921
		// (set) Token: 0x06000C99 RID: 3225 RVA: 0x0003F729 File Offset: 0x0003D929
		public CardWidget CardWidget { get; private set; }

		// Token: 0x06000C9A RID: 3226 RVA: 0x0003F732 File Offset: 0x0003D932
		private void Awake()
		{
			this.CardWidget = base.GetComponent<CardWidget>();
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x06000C9B RID: 3227 RVA: 0x0003F740 File Offset: 0x0003D940
		public Card Card
		{
			get
			{
				return this.CardWidget.Card;
			}
		}

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x06000C9C RID: 3228 RVA: 0x0003F750 File Offset: 0x0003D950
		// (remove) Token: 0x06000C9D RID: 3229 RVA: 0x0003F788 File Offset: 0x0003D988
		public event EventHandler SelectedChanged;

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x06000C9E RID: 3230 RVA: 0x0003F7BD File Offset: 0x0003D9BD
		// (set) Token: 0x06000C9F RID: 3231 RVA: 0x0003F7C5 File Offset: 0x0003D9C5
		public bool IsSelected { get; private set; }

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x06000CA0 RID: 3232 RVA: 0x0003F7CE File Offset: 0x0003D9CE
		// (set) Token: 0x06000CA1 RID: 3233 RVA: 0x0003F7D6 File Offset: 0x0003D9D6
		public GameObject SelectParticle { get; set; }

		// Token: 0x06000CA2 RID: 3234 RVA: 0x0003F7DF File Offset: 0x0003D9DF
		public void SetSelected(bool select, bool notify = true)
		{
			this.IsSelected = select;
			this.SelectParticle.SetActive(this.IsSelected);
			if (notify)
			{
				EventHandler selectedChanged = this.SelectedChanged;
				if (selectedChanged == null)
				{
					return;
				}
				selectedChanged.Invoke(this, EventArgs.Empty);
			}
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x0003F812 File Offset: 0x0003DA12
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				this.IsSelected = !this.IsSelected;
				this.SelectParticle.SetActive(this.IsSelected);
				EventHandler selectedChanged = this.SelectedChanged;
				if (selectedChanged == null)
				{
					return;
				}
				selectedChanged.Invoke(this, EventArgs.Empty);
			}
		}

		// Token: 0x06000CA4 RID: 3236 RVA: 0x0003F852 File Offset: 0x0003DA52
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.CardWidget.ShowTooltip();
		}

		// Token: 0x06000CA5 RID: 3237 RVA: 0x0003F85F File Offset: 0x0003DA5F
		public void OnPointerExit(PointerEventData eventData)
		{
			this.CardWidget.HideTooltip();
		}
	}
}
