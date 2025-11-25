using System;
using Cysharp.Threading.Tasks;
using LBoL.Core.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class LicensesPanel : UiPanel, IInputActionHandler
	{
		private void Awake()
		{
			this.returnArea.onClick.AddListener(new UnityAction(base.Hide));
			this.returnButton.onClick.AddListener(new UnityAction(base.Hide));
			this._canvasGroup = base.GetComponent<CanvasGroup>();
		}
		public override async UniTask InitializeAsync()
		{
			TextMeshProUGUI textMeshProUGUI = this.contentText;
			string text = await StreamingAssetsHelper.ReadAllTextAsync("Licenses");
			textMeshProUGUI.text = text;
			textMeshProUGUI = null;
		}
		protected override void OnShowing()
		{
			this._canvasGroup.interactable = true;
			UiManager.PushActionHandler(this);
		}
		protected override void OnHiding()
		{
			this._canvasGroup.interactable = false;
			UiManager.PopActionHandler(this);
		}
		void IInputActionHandler.OnCancel()
		{
			base.Hide();
		}
		private const string Header = "\n\n * * * Software used by this game * * *\n";
		[SerializeField]
		private TextMeshProUGUI contentText;
		[SerializeField]
		private Button returnArea;
		[SerializeField]
		private Button returnButton;
		private CanvasGroup _canvasGroup;
	}
}
