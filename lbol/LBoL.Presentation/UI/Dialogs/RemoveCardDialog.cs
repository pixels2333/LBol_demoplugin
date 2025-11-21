using System;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Dialogs
{
	// Token: 0x020000DC RID: 220
	public class RemoveCardDialog : UiDialog<RemoveCardContent>, IInputActionHandler
	{
		// Token: 0x17000238 RID: 568
		// (get) Token: 0x06000D41 RID: 3393 RVA: 0x00040CCE File Offset: 0x0003EECE
		// (set) Token: 0x06000D42 RID: 3394 RVA: 0x00040CD6 File Offset: 0x0003EED6
		public DialogResult Result { get; private set; } = DialogResult.Cancel;

		// Token: 0x06000D43 RID: 3395 RVA: 0x00040CE0 File Offset: 0x0003EEE0
		public void Awake()
		{
			this.confirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.OnCancel));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x06000D44 RID: 3396 RVA: 0x00040D33 File Offset: 0x0003EF33
		public override void OnLocaleChanged()
		{
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x00040D38 File Offset: 0x0003EF38
		protected override void OnShowing(RemoveCardContent payload)
		{
			if (payload.Card.Unremovable)
			{
				Debug.LogError("Should not remove an unremovable card: " + payload.Card.DebugName + ".");
			}
			this._content = payload;
			this.card.Card = payload.Card.Clone(true);
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x06000D46 RID: 3398 RVA: 0x00040DA4 File Offset: 0x0003EFA4
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

		// Token: 0x06000D47 RID: 3399 RVA: 0x00040DEF File Offset: 0x0003EFEF
		protected override void OnHided()
		{
			this._content = null;
			this.card.Card = null;
		}

		// Token: 0x06000D48 RID: 3400 RVA: 0x00040E04 File Offset: 0x0003F004
		public void OnConfirm()
		{
			this.Result = DialogResult.Confirm;
			base.Hide();
		}

		// Token: 0x06000D49 RID: 3401 RVA: 0x00040E13 File Offset: 0x0003F013
		public void OnCancel()
		{
			this.Result = DialogResult.Cancel;
			base.Hide();
		}

		// Token: 0x04000A04 RID: 2564
		[SerializeField]
		private CardWidget card;

		// Token: 0x04000A05 RID: 2565
		[SerializeField]
		private Button confirmButton;

		// Token: 0x04000A06 RID: 2566
		[SerializeField]
		private Button cancelButton;

		// Token: 0x04000A07 RID: 2567
		private RemoveCardContent _content;

		// Token: 0x04000A08 RID: 2568
		private CanvasGroup _canvasGroup;
	}
}
