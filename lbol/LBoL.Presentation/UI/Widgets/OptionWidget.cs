using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Dialogs;
using LBoL.Presentation.UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000069 RID: 105
	public class OptionWidget : MonoBehaviour
	{
		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x060005A3 RID: 1443 RVA: 0x00018519 File Offset: 0x00016719
		// (set) Token: 0x060005A4 RID: 1444 RVA: 0x00018521 File Offset: 0x00016721
		public bool isSeijaTrade
		{
			get
			{
				return this._isSeijaTrade;
			}
			set
			{
				if (this._isSeijaTrade != value)
				{
					this.tradeImage.gameObject.SetActive(value);
					this._isSeijaTrade = value;
				}
			}
		}

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x060005A5 RID: 1445 RVA: 0x00018544 File Offset: 0x00016744
		public DialogOption Option
		{
			get
			{
				return this._option;
			}
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x060005A6 RID: 1446 RVA: 0x0001854C File Offset: 0x0001674C
		public IEnumerable<Card> SourceCard
		{
			get
			{
				return this._sourceCard;
			}
		}

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x060005A7 RID: 1447 RVA: 0x00018554 File Offset: 0x00016754
		public IEnumerable<Exhibit> SourceExhibit
		{
			get
			{
				return this._sourceExhibit;
			}
		}

		// Token: 0x170000FD RID: 253
		// (get) Token: 0x060005A8 RID: 1448 RVA: 0x0001855C File Offset: 0x0001675C
		// (set) Token: 0x060005A9 RID: 1449 RVA: 0x00018564 File Offset: 0x00016764
		public bool IsThisSourceActive { get; set; }

		// Token: 0x060005AA RID: 1450 RVA: 0x0001856D File Offset: 0x0001676D
		private void Awake()
		{
			this.tooltipButton.onClick.AddListener(new UnityAction(this.UI_TooltipButtonClick));
		}

		// Token: 0x060005AB RID: 1451 RVA: 0x0001858B File Offset: 0x0001678B
		private void UI_TooltipButtonClick()
		{
			if (this.IsThisSourceActive)
			{
				UiManager.GetPanel<VnPanel>().ClearOptionSource();
				return;
			}
			UiManager.GetPanel<VnPanel>().ShowOptionSource(this);
		}

		// Token: 0x060005AC RID: 1452 RVA: 0x000185AB File Offset: 0x000167AB
		public void AddListener(UnityAction call)
		{
			this.mainButton.onClick.AddListener(call);
		}

		// Token: 0x060005AD RID: 1453 RVA: 0x000185BE File Offset: 0x000167BE
		public void SetOptionData(DialogOption option)
		{
			this._option = option;
			this.UpdateData();
		}

		// Token: 0x060005AE RID: 1454 RVA: 0x000185D0 File Offset: 0x000167D0
		public void UpdateData()
		{
			bool showRandomResult = GameMaster.ShowRandomResult;
			this._sourceCard.Clear();
			this._sourceExhibit.Clear();
			if (this._option != null)
			{
				this._sourceCard.AddRange(this._option.Data.GetCards(showRandomResult));
				this._sourceExhibit.AddRange(this._option.Data.GetExhibits(showRandomResult));
			}
			if (this._sourceCard.Count > 0 || this._sourceExhibit.Count > 0)
			{
				this.tooltipButton.gameObject.SetActive(true);
				return;
			}
			this.tooltipButton.gameObject.SetActive(false);
		}

		// Token: 0x0400035A RID: 858
		[SerializeField]
		private Button mainButton;

		// Token: 0x0400035B RID: 859
		[SerializeField]
		private Button tooltipButton;

		// Token: 0x0400035C RID: 860
		[SerializeField]
		private Image tradeImage;

		// Token: 0x0400035D RID: 861
		private bool _isSeijaTrade;

		// Token: 0x0400035E RID: 862
		private DialogOption _option;

		// Token: 0x0400035F RID: 863
		private readonly List<Card> _sourceCard = new List<Card>();

		// Token: 0x04000360 RID: 864
		private readonly List<Exhibit> _sourceExhibit = new List<Exhibit>();
	}
}
