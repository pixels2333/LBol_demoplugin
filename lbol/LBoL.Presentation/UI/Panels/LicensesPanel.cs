using System;
using Cysharp.Threading.Tasks;
using LBoL.Core.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200009F RID: 159
	public class LicensesPanel : UiPanel, IInputActionHandler
	{
		// Token: 0x0600083C RID: 2108 RVA: 0x00027C4C File Offset: 0x00025E4C
		private void Awake()
		{
			this.returnArea.onClick.AddListener(new UnityAction(base.Hide));
			this.returnButton.onClick.AddListener(new UnityAction(base.Hide));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x00027CA0 File Offset: 0x00025EA0
		public override async UniTask InitializeAsync()
		{
			TextMeshProUGUI textMeshProUGUI = this.contentText;
			string text = await StreamingAssetsHelper.ReadAllTextAsync("Licenses");
			textMeshProUGUI.text = text;
			textMeshProUGUI = null;
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x00027CE3 File Offset: 0x00025EE3
		protected override void OnShowing()
		{
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x00027CF7 File Offset: 0x00025EF7
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x00027D0B File Offset: 0x00025F0B
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}

		// Token: 0x040005E2 RID: 1506
		private const string Header = "\n\n * * * Software used by this game * * *\n";

		// Token: 0x040005E3 RID: 1507
		[SerializeField]
		private TextMeshProUGUI contentText;

		// Token: 0x040005E4 RID: 1508
		[SerializeField]
		private Button returnArea;

		// Token: 0x040005E5 RID: 1509
		[SerializeField]
		private Button returnButton;

		// Token: 0x040005E6 RID: 1510
		private CanvasGroup _canvasGroup;
	}
}
