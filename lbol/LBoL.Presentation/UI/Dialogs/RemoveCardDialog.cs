using System;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Dialogs
{
	public class RemoveCardDialog : UiDialog<RemoveCardContent>, IInputActionHandler
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
		private Button confirmButton;
		[SerializeField]
		private Button cancelButton;
		private RemoveCardContent _content;
		private CanvasGroup _canvasGroup;
	}
}
