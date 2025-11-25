using System;
using System.Runtime.CompilerServices;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Dialogs
{
	public class MessageDialog : UiDialog<MessageContent>, IInputActionHandler
	{
		public DialogResult Result { get; private set; } = DialogResult.Cancel;
		public void Awake()
		{
			this.singleConfirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.confirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.OnCancel));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}
		public override void OnLocaleChanged()
		{
		}
		protected override void OnShowing(MessageContent payload)
		{
			this._content = payload;
			this.mainText.text = MessageDialog.<OnShowing>g__GetTextContent|13_0(payload.Text, payload.TextKey, payload.TextArguments) ?? "<null>";
			this.subText.text = MessageDialog.<OnShowing>g__GetTextContent|13_0(payload.SubText, payload.SubTextKey, payload.SubTextArguments);
			if (payload.Buttons == DialogButtons.Confirm)
			{
				this.singleConfirmButton.gameObject.SetActive(true);
				this.confirmButton.gameObject.SetActive(false);
				this.cancelButton.gameObject.SetActive(false);
			}
			else
			{
				this.singleConfirmButton.gameObject.SetActive(false);
				this.confirmButton.gameObject.SetActive(true);
				this.cancelButton.gameObject.SetActive(true);
			}
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
			Action onConfirm = this._content.OnConfirm;
			Action onCancel = this._content.OnCancel;
			Action action = onConfirm;
			Action action2 = onCancel;
			this._content = null;
			DialogResult result = this.Result;
			if (result != DialogResult.Confirm)
			{
				if (result != DialogResult.Cancel)
				{
					throw new ArgumentOutOfRangeException();
				}
				if (action2 != null)
				{
					action2.Invoke();
					return;
				}
			}
			else if (action != null)
			{
				action.Invoke();
				return;
			}
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
		[CompilerGenerated]
		internal static string <OnShowing>g__GetTextContent|13_0(string text, string textKey, object[] arguments)
		{
			if (text != null)
			{
				return text;
			}
			if (textKey != null)
			{
				string text2 = ("MessageDialog." + textKey).Localize(true);
				if (arguments != null)
				{
					try
					{
						return string.Format(text2, arguments);
					}
					catch (Exception ex)
					{
						Debug.LogError(ex);
						return "<Error>";
					}
					return text2;
				}
				return text2;
			}
			return null;
		}
		[SerializeField]
		private TextMeshProUGUI mainText;
		[SerializeField]
		private TextMeshProUGUI subText;
		[SerializeField]
		private Button singleConfirmButton;
		[SerializeField]
		private Button confirmButton;
		[SerializeField]
		private Button cancelButton;
		private MessageContent _content;
		private CanvasGroup _canvasGroup;
	}
}
