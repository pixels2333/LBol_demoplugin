using System;
using LBoL.ConfigData;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LBoL.Presentation.UI.Widgets
{
	public class MusicWidget : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public BgmConfig Config
		{
			get
			{
				return this._config;
			}
		}
		public bool Interactable { get; set; } = true;
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
		private void Awake()
		{
			this.IsSelect = false;
		}
		public void SetMusic(BgmConfig config, int number)
		{
			this.numberText.text = string.Format("No.{0}", number);
			this.nameText.text = config.TrackName;
			this._config = config;
		}
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
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.enterParticle.alpha = 1f;
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.enterParticle.alpha = 0f;
		}
		public void PlayThis()
		{
			UiManager.GetPanel<MusicRoomPanel>().PlayMusic(this._config);
			this.IsSelect = true;
			this.enterParticle.alpha = 0f;
		}
		[SerializeField]
		private CanvasGroup selectParticle;
		[SerializeField]
		private CanvasGroup enterParticle;
		[SerializeField]
		private GameObject selectIcon;
		[SerializeField]
		private TextMeshProUGUI numberText;
		[SerializeField]
		private TextMeshProUGUI nameText;
		private BgmConfig _config;
		private bool _isSelect;
	}
}
