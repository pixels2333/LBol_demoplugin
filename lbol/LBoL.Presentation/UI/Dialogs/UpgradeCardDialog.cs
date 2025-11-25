using System;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Dialogs
{
	public class UpgradeCardDialog : UiDialog<UpgradeCardContent>, IInputActionHandler
	{
		public DialogResult Result { get; private set; } = DialogResult.Cancel;
		public void Awake()
		{
			this.confirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.OnCancel));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}
		public override void OnLocaleChanged()
		{
		}
		protected override void OnShowing(UpgradeCardContent payload)
		{
			if (!payload.Card.CanUpgrade)
			{
				throw new InvalidOperationException("Cannot upgrade " + payload.Card.DebugName);
			}
			this._content = payload;
			this.card.Card = payload.Card.Clone(true);
			Card card = payload.Card.Clone(true);
			card.Upgrade();
			this.upgradedCard.Card = card;
			if (payload.Price > 0)
			{
				this.moneyText.text = string.Format("Cards.UpgradePrice".Localize(true), payload.Price, payload.Money);
				this.moneyText.gameObject.SetActive(true);
				this.confirmButton.interactable = payload.Price <= payload.Money;
			}
			else
			{
				this.moneyText.gameObject.SetActive(false);
				this.confirmButton.interactable = true;
			}
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
			DialogResult result = this.Result;
			if (result != DialogResult.Confirm)
			{
				if (result != DialogResult.Cancel)
				{
					throw new ArgumentOutOfRangeException();
				}
				return;
			}
			else
			{
				Action onConfirm = this._content.OnConfirm;
				if (onConfirm == null)
				{
					return;
				}
				onConfirm.Invoke();
				return;
			}
		}
		protected override void OnHided()
		{
			this._content = null;
			this.card.Card = null;
			this.upgradedCard.Card = null;
		}
		public void OnConfirm()
		{
			this.Result = DialogResult.Confirm;
			base.Hide();
		}
		public void OnCancel()
		{
			this.Result = DialogResult.Cancel;
			base.Hide();
		}
		[SerializeField]
		private CardWidget card;
		[SerializeField]
		private CardWidget upgradedCard;
		[SerializeField]
		private Button confirmButton;
		[SerializeField]
		private Button cancelButton;
		[SerializeField]
		private TextMeshProUGUI moneyText;
		private UpgradeCardContent _content;
		private CanvasGroup _canvasGroup;
	}
}
