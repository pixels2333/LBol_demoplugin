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
	// Token: 0x020000E0 RID: 224
	public class UpgradeCardDialog : UiDialog<UpgradeCardContent>, IInputActionHandler
	{
		// Token: 0x17000241 RID: 577
		// (get) Token: 0x06000D65 RID: 3429 RVA: 0x00041081 File Offset: 0x0003F281
		// (set) Token: 0x06000D66 RID: 3430 RVA: 0x00041089 File Offset: 0x0003F289
		public DialogResult Result { get; private set; } = DialogResult.Cancel;

		// Token: 0x06000D67 RID: 3431 RVA: 0x00041094 File Offset: 0x0003F294
		public void Awake()
		{
			this.confirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.OnCancel));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x06000D68 RID: 3432 RVA: 0x000410E7 File Offset: 0x0003F2E7
		public override void OnLocaleChanged()
		{
		}

		// Token: 0x06000D69 RID: 3433 RVA: 0x000410EC File Offset: 0x0003F2EC
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

		// Token: 0x06000D6A RID: 3434 RVA: 0x000411F8 File Offset: 0x0003F3F8
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

		// Token: 0x06000D6B RID: 3435 RVA: 0x00041243 File Offset: 0x0003F443
		protected override void OnHided()
		{
			this._content = null;
			this.card.Card = null;
			this.upgradedCard.Card = null;
		}

		// Token: 0x06000D6C RID: 3436 RVA: 0x00041264 File Offset: 0x0003F464
		public void OnConfirm()
		{
			this.Result = DialogResult.Confirm;
			base.Hide();
		}

		// Token: 0x06000D6D RID: 3437 RVA: 0x00041273 File Offset: 0x0003F473
		public void OnCancel()
		{
			this.Result = DialogResult.Cancel;
			base.Hide();
		}

		// Token: 0x04000A19 RID: 2585
		[SerializeField]
		private CardWidget card;

		// Token: 0x04000A1A RID: 2586
		[SerializeField]
		private CardWidget upgradedCard;

		// Token: 0x04000A1B RID: 2587
		[SerializeField]
		private Button confirmButton;

		// Token: 0x04000A1C RID: 2588
		[SerializeField]
		private Button cancelButton;

		// Token: 0x04000A1D RID: 2589
		[SerializeField]
		private TextMeshProUGUI moneyText;

		// Token: 0x04000A1E RID: 2590
		private UpgradeCardContent _content;

		// Token: 0x04000A1F RID: 2591
		private CanvasGroup _canvasGroup;
	}
}
