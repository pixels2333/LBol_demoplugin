using System;
using System.Runtime.CompilerServices;
using LBoL.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Dialogs
{
	// Token: 0x020000DA RID: 218
	public class MessageDialog : UiDialog<MessageContent>, IInputActionHandler
	{
		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06000D32 RID: 3378 RVA: 0x00040A4A File Offset: 0x0003EC4A
		// (set) Token: 0x06000D33 RID: 3379 RVA: 0x00040A52 File Offset: 0x0003EC52
		public DialogResult Result { get; private set; } = DialogResult.Cancel;

		// Token: 0x06000D34 RID: 3380 RVA: 0x00040A5C File Offset: 0x0003EC5C
		public void Awake()
		{
			this.singleConfirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.confirmButton.onClick.AddListener(new UnityAction(this.OnConfirm));
			this.cancelButton.onClick.AddListener(new UnityAction(this.OnCancel));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x06000D35 RID: 3381 RVA: 0x00040ACC File Offset: 0x0003ECCC
		public override void OnLocaleChanged()
		{
		}

		// Token: 0x06000D36 RID: 3382 RVA: 0x00040AD0 File Offset: 0x0003ECD0
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

		// Token: 0x06000D37 RID: 3383 RVA: 0x00040BB4 File Offset: 0x0003EDB4
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

		// Token: 0x06000D38 RID: 3384 RVA: 0x00040C1E File Offset: 0x0003EE1E
		public void OnConfirm()
		{
			this.Result = DialogResult.Confirm;
			base.Hide();
		}

		// Token: 0x06000D39 RID: 3385 RVA: 0x00040C2D File Offset: 0x0003EE2D
		public void OnCancel()
		{
			this.Result = DialogResult.Cancel;
			base.Hide();
		}

		// Token: 0x06000D3B RID: 3387 RVA: 0x00040C4C File Offset: 0x0003EE4C
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

		// Token: 0x040009FA RID: 2554
		[SerializeField]
		private TextMeshProUGUI mainText;

		// Token: 0x040009FB RID: 2555
		[SerializeField]
		private TextMeshProUGUI subText;

		// Token: 0x040009FC RID: 2556
		[SerializeField]
		private Button singleConfirmButton;

		// Token: 0x040009FD RID: 2557
		[SerializeField]
		private Button confirmButton;

		// Token: 0x040009FE RID: 2558
		[SerializeField]
		private Button cancelButton;

		// Token: 0x040009FF RID: 2559
		private MessageContent _content;

		// Token: 0x04000A00 RID: 2560
		private CanvasGroup _canvasGroup;
	}
}
