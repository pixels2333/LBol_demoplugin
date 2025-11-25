using System;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Dialogs
{
	public class TransformCardDialog : UiDialog<TransformCardContent>, IInputActionHandler
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
			this.transformCard.Card = null;
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
		private CardWidget transformCard;
		[SerializeField]
		private GameObject randomCard;
		[SerializeField]
		private Button confirmButton;
		[SerializeField]
		private Button cancelButton;
		private TransformCardContent _content;
		private CanvasGroup _canvasGroup;
	}
}
