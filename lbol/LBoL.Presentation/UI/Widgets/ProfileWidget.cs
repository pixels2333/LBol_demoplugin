using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core;
using LBoL.Core.SaveData;
using LBoL.Presentation.I10N;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class ProfileWidget : MonoBehaviour
	{
		private void Awake()
		{
			this.content.SetActive(false);
			this.emptyContent.SetActive(true);
		}
		public void Init(UnityAction delete, UnityAction edit)
		{
			this.deleteButton.onClick.AddListener(delete);
			this.editButton.onClick.AddListener(edit);
		}
		public ProfileSaveData Profile
		{
			get
			{
				return this._profile;
			}
		}
		public GameRunSaveData GameRun
		{
			get
			{
				return this._gameRun;
			}
		}
		public void SetSaveData(ProfileSaveData profileSaveData, GameRunSaveData gameRunSaveData)
		{
			if (this._profile == profileSaveData)
			{
				return;
			}
			this._profile = profileSaveData;
			if (profileSaveData == null)
			{
				this.content.SetActive(false);
				this.emptyContent.SetActive(true);
				this.headImage.sprite = UiManager.GetPanel<ProfilePanel>().GetHeadSprite(null);
				return;
			}
			this.content.SetActive(true);
			this.emptyContent.SetActive(false);
			this.nameText.text = profileSaveData.Name;
			this.levelText.text = string.Format("{0} {1}", "Game.UnlockLevel".Localize(true), ExpHelper.GetLevelForTotalExp(profileSaveData.Exp));
			DateTime dateTime;
			if (Utils.TryParseIso8601Timestamp(profileSaveData.CreationTimestamp, out dateTime))
			{
				this.createTimeText.text = dateTime.ToLocalTime().ToString(L10nManager.Info.Culture);
			}
			int num = profileSaveData.TotalPlaySeconds;
			if (gameRunSaveData != null)
			{
				num += gameRunSaveData.PlayedSeconds;
			}
			this.playedTimeText.text = Utils.SecondsToHHMMSS(num);
			if (gameRunSaveData != null)
			{
				try
				{
					this.headImage.sprite = UiManager.GetPanel<ProfilePanel>().GetHeadSprite(gameRunSaveData.Player.Name);
					DateTime dateTime2;
					if (Utils.TryParseIso8601Timestamp(gameRunSaveData.SaveTimestamp, out dateTime2))
					{
						this.gameRunInfoText.text = dateTime2.ToLocalTime().ToString("F");
					}
					else
					{
						this.gameRunInfoText.text = "";
					}
					return;
				}
				catch (Exception)
				{
					this.gameRunInfoText.text = "";
					return;
				}
			}
			this.gameRunInfoText.text = "";
		}
		public void Animate(float delay, bool isOut)
		{
			base.transform.DOKill(false);
			if (isOut)
			{
				this.scroll.DOSizeDelta(Vector2.zero, 0.4f, false).From(new Vector2(-2100f, 0f), true, false).SetDelay(delay)
					.SetUpdate(true)
					.SetTarget(base.transform);
				this.scrollPaper.DOScaleX(0f, 0.4f).From(1f, true, false).SetDelay(delay)
					.SetUpdate(true)
					.SetTarget(base.transform);
				this.content.GetComponent<CanvasGroup>().DOFade(0f, 0.1f).From(1f, true, false)
					.SetDelay(delay)
					.SetUpdate(true)
					.SetTarget(base.transform);
				this.emptyContent.GetComponent<CanvasGroup>().DOFade(0f, 0.1f).From(1f, true, false)
					.SetDelay(delay)
					.SetUpdate(true)
					.SetTarget(base.transform);
				return;
			}
			this.scrollPaper.DOScaleX(1f, 0.6f).From(0f, true, false).SetDelay(delay)
				.SetUpdate(true)
				.SetTarget(base.transform);
			this.content.GetComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false)
				.SetDelay(delay + 0.5f)
				.SetUpdate(true)
				.SetTarget(base.transform);
			this.emptyContent.GetComponent<CanvasGroup>().DOFade(1f, 0.2f).From(0f, true, false)
				.SetDelay(delay + 0.5f)
				.SetUpdate(true)
				.SetTarget(base.transform);
			this.scroll.DOSizeDelta(new Vector2(-2100f, 0f), 0.6f, false).From(Vector2.zero, true, false).SetDelay(delay)
				.SetUpdate(true)
				.SetTarget(base.transform);
		}
		[SerializeField]
		private GameObject content;
		[SerializeField]
		private GameObject emptyContent;
		[SerializeField]
		private TextMeshProUGUI nameText;
		[SerializeField]
		private TextMeshProUGUI levelText;
		[SerializeField]
		private TextMeshProUGUI playedTimeText;
		[SerializeField]
		private TextMeshProUGUI createTimeText;
		[SerializeField]
		private TextMeshProUGUI gameRunInfoText;
		[SerializeField]
		private Button editButton;
		[SerializeField]
		private Button deleteButton;
		[SerializeField]
		private Image headImage;
		[FormerlySerializedAs("scoll")]
		[SerializeField]
		private RectTransform scroll;
		[FormerlySerializedAs("scollPaper")]
		[SerializeField]
		private RectTransform scrollPaper;
		private ProfileSaveData _profile;
		private GameRunSaveData _gameRun;
	}
}
