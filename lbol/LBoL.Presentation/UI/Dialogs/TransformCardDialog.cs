using System;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Dialogs
{
	// Token: 0x020000DE RID: 222
	public class TransformCardDialog : UiDialog<TransformCardContent>, IInputActionHandler
	{
		// Token: 0x1700023C RID: 572
		// (get) Token: 0x06000D52 RID: 3410 RVA: 0x00040E6C File Offset: 0x0003F06C
		// (set) Token: 0x06000D53 RID: 3411 RVA: 0x00040E74 File Offset: 0x0003F074
		public DialogResult Result { get; private set; } = DialogResult.Cancel;

		// Token: 0x06000D54 RID: 3412 RVA: 0x00040E80 File Offset: 0x0003F080
		public void Awake()
		{
			this.confirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.OnCancel));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x00040ED3 File Offset: 0x0003F0D3
		public override void OnLocaleChanged()
		{
		}

		// Token: 0x06000D56 RID: 3414 RVA: 0x00040ED8 File Offset: 0x0003F0D8
		protected override void OnShowing(TransformCardContent payload)
		{
			if (payload.Card.Unremovable)
			{
				Debug.LogError("Should not transform an unremovable card: " + payload.Card.DebugName + ".");
			}
			this._content = payload;
			this.card.Card = payload.Card.Clone(true);
			if (GameMaster.ShowRandomResult)
			{
				this.randomCard.SetActive(false);
				this.transformCard.gameObject.SetActive(true);
				this.transformCard.Card = payload.TransformCard.Clone(true);
			}
			else
			{
				this.randomCard.SetActive(true);
				this.transformCard.gameObject.SetActive(false);
			}
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x06000D57 RID: 3415 RVA: 0x00040F9C File Offset: 0x0003F19C
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

		// Token: 0x06000D58 RID: 3416 RVA: 0x00040FE7 File Offset: 0x0003F1E7
		protected override void OnHided()
		{
			this._content = null;
			this.card.Card = null;
			this.transformCard.Card = null;
		}

		// Token: 0x06000D59 RID: 3417 RVA: 0x00041008 File Offset: 0x0003F208
		public void OnConfirm()
		{
			this.Result = DialogResult.Confirm;
			base.Hide();
		}

		// Token: 0x06000D5A RID: 3418 RVA: 0x00041017 File Offset: 0x0003F217
		public void OnCancel()
		{
			this.Result = DialogResult.Cancel;
			base.Hide();
		}

		// Token: 0x04000A0D RID: 2573
		[SerializeField]
		private CardWidget card;

		// Token: 0x04000A0E RID: 2574
		[SerializeField]
		private CardWidget transformCard;

		// Token: 0x04000A0F RID: 2575
		[SerializeField]
		private GameObject randomCard;

		// Token: 0x04000A10 RID: 2576
		[SerializeField]
		private Button confirmButton;

		// Token: 0x04000A11 RID: 2577
		[SerializeField]
		private Button cancelButton;

		// Token: 0x04000A12 RID: 2578
		private TransformCardContent _content;

		// Token: 0x04000A13 RID: 2579
		private CanvasGroup _canvasGroup;
	}
}
