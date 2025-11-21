using System;
using LBoL.Core;
using UnityEngine;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x02000093 RID: 147
	public class EntryPanel : UiPanel
	{
		// Token: 0x060007BA RID: 1978 RVA: 0x00024350 File Offset: 0x00022550
		protected override void OnShown()
		{
		}

		// Token: 0x060007BB RID: 1979 RVA: 0x00024352 File Offset: 0x00022552
		private void Update()
		{
			this.info.alpha = Mathf.Abs(Mathf.Sin(Time.time));
		}

		// Token: 0x060007BC RID: 1980 RVA: 0x00024370 File Offset: 0x00022570
		public void UI_StartGame()
		{
			if (Singleton<GameMaster>.Instance.CurrentSaveIndex != null)
			{
				UiManager.GetPanel<MainMenuPanel>().Show();
				return;
			}
			UiManager.Show<ProfilePanel>();
		}

		// Token: 0x0400051B RID: 1307
		[SerializeField]
		private CanvasGroup info;
	}
}
