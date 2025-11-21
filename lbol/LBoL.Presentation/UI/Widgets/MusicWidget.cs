using System;
using LBoL.ConfigData;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000068 RID: 104
	public class MusicWidget : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000597 RID: 1431 RVA: 0x000183D2 File Offset: 0x000165D2
		public BgmConfig Config
		{
			get
			{
				return this._config;
			}
		}

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x06000598 RID: 1432 RVA: 0x000183DA File Offset: 0x000165DA
		// (set) Token: 0x06000599 RID: 1433 RVA: 0x000183E2 File Offset: 0x000165E2
		public bool Interactable { get; set; } = true;

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x0600059A RID: 1434 RVA: 0x000183EB File Offset: 0x000165EB
		// (set) Token: 0x0600059B RID: 1435 RVA: 0x000183F3 File Offset: 0x000165F3
		public bool IsSelect
		{
			get
			{
				return this._isSelect;
			}
			set
			{
				this._isSelect = value;
				this.selectParticle.alpha = (float)(this._isSelect ? 1 : 0);
				this.selectIcon.SetActive(this._isSelect);
			}
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x00018425 File Offset: 0x00016625
		private void Awake()
		{
			this.IsSelect = false;
		}

		// Token: 0x0600059D RID: 1437 RVA: 0x0001842E File Offset: 0x0001662E
		public void SetMusic(BgmConfig config, int number)
		{
			this.numberText.text = string.Format("No.{0}", number);
			this.nameText.text = config.TrackName;
			this._config = config;
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x00018464 File Offset: 0x00016664
		public void OnPointerClick(PointerEventData eventData)
		{
			if (!this.Interactable)
			{
				return;
			}
			MusicRoomPanel panel = UiManager.GetPanel<MusicRoomPanel>();
			if (!this._isSelect)
			{
				panel.PlayMusic(this._config);
			}
			else
			{
				panel.StopMusic();
			}
			this.IsSelect = !this._isSelect;
			this.enterParticle.alpha = 0f;
		}

		// Token: 0x0600059F RID: 1439 RVA: 0x000184BC File Offset: 0x000166BC
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.enterParticle.alpha = 1f;
		}

		// Token: 0x060005A0 RID: 1440 RVA: 0x000184CE File Offset: 0x000166CE
		public void OnPointerExit(PointerEventData eventData)
		{
			this.enterParticle.alpha = 0f;
		}

		// Token: 0x060005A1 RID: 1441 RVA: 0x000184E0 File Offset: 0x000166E0
		public void PlayThis()
		{
			UiManager.GetPanel<MusicRoomPanel>().PlayMusic(this._config);
			this.IsSelect = true;
			this.enterParticle.alpha = 0f;
		}

		// Token: 0x04000352 RID: 850
		[SerializeField]
		private CanvasGroup selectParticle;

		// Token: 0x04000353 RID: 851
		[SerializeField]
		private CanvasGroup enterParticle;

		// Token: 0x04000354 RID: 852
		[SerializeField]
		private GameObject selectIcon;

		// Token: 0x04000355 RID: 853
		[SerializeField]
		private TextMeshProUGUI numberText;

		// Token: 0x04000356 RID: 854
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000357 RID: 855
		private BgmConfig _config;

		// Token: 0x04000358 RID: 856
		private bool _isSelect;
	}
}
