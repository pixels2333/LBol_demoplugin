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
	// Token: 0x0200006A RID: 106
	public class ProfileWidget : MonoBehaviour
	{
		// Token: 0x060005B0 RID: 1456 RVA: 0x00018696 File Offset: 0x00016896
		private void Awake()
		{
			this.content.SetActive(false);
			this.emptyContent.SetActive(true);
		}

		// Token: 0x060005B1 RID: 1457 RVA: 0x000186B0 File Offset: 0x000168B0
		public void Init(UnityAction delete, UnityAction edit)
		{
			this.deleteButton.onClick.AddListener(delete);
			this.editButton.onClick.AddListener(edit);
		}

		// Token: 0x170000FE RID: 254
		// (get) Token: 0x060005B2 RID: 1458 RVA: 0x000186D4 File Offset: 0x000168D4
		public ProfileSaveData Profile
		{
			get
			{
				return this._profile;
			}
		}

		// Token: 0x170000FF RID: 255
		// (get) Token: 0x060005B3 RID: 1459 RVA: 0x000186DC File Offset: 0x000168DC
		public GameRunSaveData GameRun
		{
			get
			{
				return this._gameRun;
			}
		}

		// Token: 0x060005B4 RID: 1460 RVA: 0x000186E4 File Offset: 0x000168E4
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

		// Token: 0x060005B5 RID: 1461 RVA: 0x0001887C File Offset: 0x00016A7C
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

		// Token: 0x04000362 RID: 866
		[SerializeField]
		private GameObject content;

		// Token: 0x04000363 RID: 867
		[SerializeField]
		private GameObject emptyContent;

		// Token: 0x04000364 RID: 868
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x04000365 RID: 869
		[SerializeField]
		private TextMeshProUGUI levelText;

		// Token: 0x04000366 RID: 870
		[SerializeField]
		private TextMeshProUGUI playedTimeText;

		// Token: 0x04000367 RID: 871
		[SerializeField]
		private TextMeshProUGUI createTimeText;

		// Token: 0x04000368 RID: 872
		[SerializeField]
		private TextMeshProUGUI gameRunInfoText;

		// Token: 0x04000369 RID: 873
		[SerializeField]
		private Button editButton;

		// Token: 0x0400036A RID: 874
		[SerializeField]
		private Button deleteButton;

		// Token: 0x0400036B RID: 875
		[SerializeField]
		private Image headImage;

		// Token: 0x0400036C RID: 876
		[FormerlySerializedAs("scoll")]
		[SerializeField]
		private RectTransform scroll;

		// Token: 0x0400036D RID: 877
		[FormerlySerializedAs("scollPaper")]
		[SerializeField]
		private RectTransform scrollPaper;

		// Token: 0x0400036E RID: 878
		private ProfileSaveData _profile;

		// Token: 0x0400036F RID: 879
		private GameRunSaveData _gameRun;
	}
}
