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
	public class OptionWidget : MonoBehaviour
	{
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
		public DialogOption Option
		{
			get
			{
				return this._option;
			}
		}
		public IEnumerable<Card> SourceCard
		{
			get
			{
				return this._sourceCard;
			}
		}
		public IEnumerable<Exhibit> SourceExhibit
		{
			get
			{
				return this._sourceExhibit;
			}
		}
		public bool IsThisSourceActive { get; set; }
		private void Awake()
		{
			this.tooltipButton.onClick.AddListener(new UnityAction(this.UI_TooltipButtonClick));
		}
		private void UI_TooltipButtonClick()
		{
			if (this.IsThisSourceActive)
			{
				UiManager.GetPanel<VnPanel>().ClearOptionSource();
				return;
			}
			UiManager.GetPanel<VnPanel>().ShowOptionSource(this);
		}
		public void AddListener(UnityAction call)
		{
			this.mainButton.onClick.AddListener(call);
		}
		public void SetOptionData(DialogOption option)
		{
			this._option = option;
			this.UpdateData();
		}
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
		[SerializeField]
		private Button mainButton;
		[SerializeField]
		private Button tooltipButton;
		[SerializeField]
		private Image tradeImage;
		private bool _isSeijaTrade;
		private DialogOption _option;
		private readonly List<Card> _sourceCard = new List<Card>();
		private readonly List<Exhibit> _sourceExhibit = new List<Exhibit>();
	}
}
