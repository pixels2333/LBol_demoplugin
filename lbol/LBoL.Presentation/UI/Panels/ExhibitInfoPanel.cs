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
	public class ExhibitInfoPanel : UiPanel<Exhibit>, IInputActionHandler
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Top;
			}
		}
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}
		public void Awake()
		{
			this.infoPanelMask.onClick.AddListener(new UnityAction(base.Hide));
			this.leftButton.onClick.AddListener(new UnityAction(this.PageLeftClick));
			this.rightButton.onClick.AddListener(new UnityAction(this.PageRightClick));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}
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
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}
		private void SetExhibitByIndex(int index)
		{
			Exhibit exhibit = this._currentPlayerExhibits[index];
			this.SetExhibit(exhibit);
			this.SetArrows();
		}
		private void SetExhibit(Exhibit exhibit)
		{
			this.infoExhibit.Exhibit = exhibit;
			this.infoExhibitName.text = exhibit.Name;
			this.infoExhibitRarity.text = ("Rarity." + exhibit.Config.Rarity.ToString()).Localize(true);
			this.infoExhibitBg.sprite = this.boothList[exhibit.Config.Rarity] ?? this.boothList[Rarity.Common];
			this.exhibitTooltipWidget.SetExhibit(exhibit);
		}
		private void SetArrows()
		{
			this.leftButton.interactable = this._index > 0;
			this.rightButton.interactable = this._index < this._currentPlayerExhibits.Count - 1;
		}
		private void ArrowsActive(bool on)
		{
			this.leftButton.gameObject.SetActive(on);
			this.rightButton.gameObject.SetActive(on);
		}
		private void PageLeftClick()
		{
			this._index = Math.Max(this._index - 1, 0);
			this.SetExhibitByIndex(this._index);
		}
		private void PageRightClick()
		{
			this._index = Math.Min(this._index + 1, this._currentPlayerExhibits.Count);
			this.SetExhibitByIndex(this._index);
		}
		[SerializeField]
		private Button leftButton;
		[SerializeField]
		private Button rightButton;
		[SerializeField]
		private MuseumExhibitTooltip exhibitTooltipWidget;
		[SerializeField]
		private ExhibitWidget infoExhibit;
		[SerializeField]
		private TextMeshProUGUI infoExhibitName;
		[SerializeField]
		private TextMeshProUGUI infoExhibitRarity;
		[SerializeField]
		private Image infoExhibitBg;
		[SerializeField]
		private Button infoPanelMask;
		[SerializeField]
		private AssociationList<Rarity, Sprite> boothList;
		private List<Exhibit> _currentPlayerExhibits;
		private CanvasGroup _canvasGroup;
		private int _index;
	}
}
