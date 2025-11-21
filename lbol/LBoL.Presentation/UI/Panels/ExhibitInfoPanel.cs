using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Core;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x02000094 RID: 148
	public class ExhibitInfoPanel : UiPanel<Exhibit>, IInputActionHandler
	{
		// Token: 0x17000148 RID: 328
		// (get) Token: 0x060007BE RID: 1982 RVA: 0x000243A9 File Offset: 0x000225A9
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}

		// Token: 0x060007BF RID: 1983 RVA: 0x000243AC File Offset: 0x000225AC
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}

		// Token: 0x060007C0 RID: 1984 RVA: 0x000243B4 File Offset: 0x000225B4
		public void Awake()
		{
			this.infoPanelMask.onClick.AddListener(new UnityAction(base.Hide));
			this.leftButton.onClick.AddListener(new UnityAction(this.PageLeftClick));
			this.rightButton.onClick.AddListener(new UnityAction(this.PageRightClick));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x060007C1 RID: 1985 RVA: 0x00024424 File Offset: 0x00022624
		protected override void OnShowing(Exhibit payload)
		{
			this._currentPlayerExhibits = Enumerable.ToList<Exhibit>(Enumerable.Select<ExhibitWidget, Exhibit>(UiManager.GetPanel<SystemBoard>().sortedExhibitWidgets, (ExhibitWidget w) => w.Exhibit));
			if (this._currentPlayerExhibits.Contains(payload))
			{
				this._index = this._currentPlayerExhibits.IndexOf(payload);
				this.ArrowsActive(true);
				this.SetExhibitByIndex(this._index);
			}
			else
			{
				this.ArrowsActive(false);
				this.SetExhibit(payload);
			}
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x060007C2 RID: 1986 RVA: 0x000244BF File Offset: 0x000226BF
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x000244D4 File Offset: 0x000226D4
		private void SetExhibitByIndex(int index)
		{
			Exhibit exhibit = this._currentPlayerExhibits[index];
			this.SetExhibit(exhibit);
			this.SetArrows();
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x000244FC File Offset: 0x000226FC
		private void SetExhibit(Exhibit exhibit)
		{
			this.infoExhibit.Exhibit = exhibit;
			this.infoExhibitName.text = exhibit.Name;
			this.infoExhibitRarity.text = ("Rarity." + exhibit.Config.Rarity.ToString()).Localize(true);
			this.infoExhibitBg.sprite = this.boothList[exhibit.Config.Rarity] ?? this.boothList[Rarity.Common];
			this.exhibitTooltipWidget.SetExhibit(exhibit);
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x00024597 File Offset: 0x00022797
		private void SetArrows()
		{
			this.leftButton.interactable = this._index > 0;
			this.rightButton.interactable = this._index < this._currentPlayerExhibits.Count - 1;
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x000245CD File Offset: 0x000227CD
		private void ArrowsActive(bool on)
		{
			this.leftButton.gameObject.SetActive(on);
			this.rightButton.gameObject.SetActive(on);
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x000245F1 File Offset: 0x000227F1
		private void PageLeftClick()
		{
			this._index = Math.Max(this._index - 1, 0);
			this.SetExhibitByIndex(this._index);
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x00024613 File Offset: 0x00022813
		private void PageRightClick()
		{
			this._index = Math.Min(this._index + 1, this._currentPlayerExhibits.Count);
			this.SetExhibitByIndex(this._index);
		}

		// Token: 0x0400051C RID: 1308
		[SerializeField]
		private Button leftButton;

		// Token: 0x0400051D RID: 1309
		[SerializeField]
		private Button rightButton;

		// Token: 0x0400051E RID: 1310
		[SerializeField]
		private MuseumExhibitTooltip exhibitTooltipWidget;

		// Token: 0x0400051F RID: 1311
		[SerializeField]
		private ExhibitWidget infoExhibit;

		// Token: 0x04000520 RID: 1312
		[SerializeField]
		private TextMeshProUGUI infoExhibitName;

		// Token: 0x04000521 RID: 1313
		[SerializeField]
		private TextMeshProUGUI infoExhibitRarity;

		// Token: 0x04000522 RID: 1314
		[SerializeField]
		private Image infoExhibitBg;

		// Token: 0x04000523 RID: 1315
		[SerializeField]
		private Button infoPanelMask;

		// Token: 0x04000524 RID: 1316
		[SerializeField]
		private AssociationList<Rarity, Sprite> boothList;

		// Token: 0x04000525 RID: 1317
		private List<Exhibit> _currentPlayerExhibits;

		// Token: 0x04000526 RID: 1318
		private CanvasGroup _canvasGroup;

		// Token: 0x04000527 RID: 1319
		private int _index;
	}
}
