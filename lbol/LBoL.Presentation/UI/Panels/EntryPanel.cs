using System;
using LBoL.Core;
using UnityEngine;
namespace LBoL.Presentation.UI.Panels
{
	public class EntryPanel : UiPanel
	{
		protected override void OnShown()
		{
		}
		private void Update()
		{
			this.info.alpha = Mathf.Abs(Mathf.Sin(Time.time));
		}
		public void UI_StartGame()
		{
			if (Singleton<GameMaster>.Instance.CurrentSaveIndex != null)
			{
				UiManager.GetPanel<MainMenuPanel>().Show();
				return;
			}
			UiManager.Show<ProfilePanel>();
		}
		[SerializeField]
		private CanvasGroup info;
	}
}
