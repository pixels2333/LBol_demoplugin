using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Presentation.UI.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000C0 RID: 192
	public sealed class TopMessagePanel : UiPanel
	{
		// Token: 0x170001BB RID: 443
		// (get) Token: 0x06000B57 RID: 2903 RVA: 0x0003B1FB File Offset: 0x000393FB
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Topmost;
			}
		}

		// Token: 0x06000B58 RID: 2904 RVA: 0x0003B1FE File Offset: 0x000393FE
		private void ShowBgmHint(BgmConfig config)
		{
			this.bgmHint.ShowHint(config);
		}

		// Token: 0x06000B59 RID: 2905 RVA: 0x0003B20C File Offset: 0x0003940C
		private void Awake()
		{
			this._messageHeight = this.messageTemplate.rect.height;
			this.messageTemplate.gameObject.SetActive(false);
			this.achievementHintWidget.gameObject.SetActive(false);
			this.JadeBoxDefaultPosition = this.jadeBoxPanel.anchoredPosition;
			this.JadeBoxDefaultSize = this.jadeBoxPanel.sizeDelta;
			this.jadeBoxTemplate.gameObject.SetActive(false);
			this.ShowJadeBoxPanel = false;
		}

		// Token: 0x06000B5A RID: 2906 RVA: 0x0003B28E File Offset: 0x0003948E
		private void OnEnable()
		{
			AudioManager.BgmChanged += new Action<BgmConfig>(this.ShowBgmHint);
		}

		// Token: 0x06000B5B RID: 2907 RVA: 0x0003B2A1 File Offset: 0x000394A1
		private void OnDisable()
		{
			AudioManager.BgmChanged -= new Action<BgmConfig>(this.ShowBgmHint);
		}

		// Token: 0x06000B5C RID: 2908 RVA: 0x0003B2B4 File Offset: 0x000394B4
		public void ShowMessage(string content)
		{
			RectTransform message = Object.Instantiate<RectTransform>(this.messageTemplate, this.root);
			message.gameObject.SetActive(true);
			message.GetComponentInChildren<TextMeshProUGUI>().text = content;
			foreach (ValueTuple<int, RectTransform> valueTuple in this._messages.WithIndices<RectTransform>())
			{
				int item = valueTuple.Item1;
				valueTuple.Item2.DOLocalMoveY(this._messageHeight * (float)item, 0.2f, false);
			}
			this._messages.Add(message);
			CanvasGroup orAddComponent = message.gameObject.GetOrAddComponent<CanvasGroup>();
			DOTween.Sequence().Join(message.DOLocalMoveX(0f, 0.4f, false).From(1000f, true, false)).Join(orAddComponent.DOFade(1f, 0.4f).From(0.5f, true, false))
				.AppendInterval(3f)
				.Append(orAddComponent.DOFade(0f, 1f))
				.OnComplete(delegate
				{
					this._messages.Remove(message);
				});
		}

		// Token: 0x06000B5D RID: 2909 RVA: 0x0003B408 File Offset: 0x00039608
		public void UnlockAchievement(string key)
		{
			this._achievementQueue.Enqueue(key);
			if (!this._isShowingHints)
			{
				this._isShowingHints = true;
				this._showHintsCoroutine = base.StartCoroutine(this.ShowAchievementHints());
			}
		}

		// Token: 0x06000B5E RID: 2910 RVA: 0x0003B437 File Offset: 0x00039637
		private IEnumerator ShowAchievementHints()
		{
			int slot = 0;
			while (this._isShowingHints)
			{
				if (this.currentHint < this.maxHints && this._achievementQueue.Count > 0)
				{
					string text = this._achievementQueue.Dequeue();
					this.ShowAchievementHint(text, slot);
					this.currentHint++;
					if (slot < this.maxHints - 1)
					{
						int num = slot;
						slot = num + 1;
					}
					else
					{
						slot = 0;
					}
				}
				yield return new WaitForSecondsRealtime(this.hintDelay);
			}
			yield break;
		}

		// Token: 0x06000B5F RID: 2911 RVA: 0x0003B448 File Offset: 0x00039648
		private void ShowAchievementHint(string key, int hintIndex)
		{
			AudioManager.PlayUi("UnlockAchievement", false);
			RectTransform achievementHint = Object.Instantiate<RectTransform>(this.achievementHintWidget, this.root);
			achievementHint.GetComponentInChildren<AchievementHintWidget>().SetAchievementHint(key);
			achievementHint.gameObject.SetActive(true);
			CanvasGroup orAddComponent = achievementHint.gameObject.GetOrAddComponent<CanvasGroup>();
			orAddComponent.alpha = 0f;
			float num = (float)(1250 - hintIndex * 250);
			DOTween.Sequence().Append(achievementHint.transform.DOLocalMoveY(num, 0.5f, false).SetEase(Ease.OutCubic)).Join(orAddComponent.DOFade(1f, 0.5f).From(0f, true, false))
				.AppendInterval(3f)
				.Append(orAddComponent.DOFade(0f, 0.5f))
				.OnComplete(delegate
				{
					this.currentHint--;
					Object.Destroy(achievementHint.gameObject);
					if (this._achievementQueue.Count == 0)
					{
						this._isShowingHints = false;
					}
				})
				.SetUpdate(true);
		}

		// Token: 0x06000B60 RID: 2912 RVA: 0x0003B551 File Offset: 0x00039751
		public void ClearAllHints()
		{
			if (this._showHintsCoroutine != null)
			{
				base.StopCoroutine(this._showHintsCoroutine);
			}
			this._achievementQueue.Clear();
			this._isShowingHints = false;
		}

		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06000B61 RID: 2913 RVA: 0x0003B579 File Offset: 0x00039779
		private RectTransform JadeBoxHint
		{
			get
			{
				return UiManager.GetPanel<SystemBoard>().jadeBoxHint;
			}
		}

		// Token: 0x170001BD RID: 445
		// (get) Token: 0x06000B62 RID: 2914 RVA: 0x0003B585 File Offset: 0x00039785
		// (set) Token: 0x06000B63 RID: 2915 RVA: 0x0003B58D File Offset: 0x0003978D
		private Vector2 JadeBoxDefaultPosition { get; set; }

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06000B64 RID: 2916 RVA: 0x0003B596 File Offset: 0x00039796
		// (set) Token: 0x06000B65 RID: 2917 RVA: 0x0003B59E File Offset: 0x0003979E
		private Vector2 JadeBoxDefaultSize { get; set; }

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06000B66 RID: 2918 RVA: 0x0003B5A7 File Offset: 0x000397A7
		// (set) Token: 0x06000B67 RID: 2919 RVA: 0x0003B5AF File Offset: 0x000397AF
		public bool ShowJadeBoxPanel
		{
			get
			{
				return this._showJadeBoxPanel;
			}
			set
			{
				this._showJadeBoxPanel = value;
				this.jadeBoxPanel.gameObject.SetActive(value);
			}
		}

		// Token: 0x06000B68 RID: 2920 RVA: 0x0003B5CC File Offset: 0x000397CC
		public void SetJadeBoxes(List<JadeBox> jadeBoxes)
		{
			this.ClearJadeBoxWidgets();
			int num = Math.Max(1, jadeBoxes.Count);
			if (num > 5)
			{
				Debug.LogError("玉匣太多了，UI显示会有问题。");
			}
			float num2 = (float)(num - 1) * 220f;
			this.jadeBoxPanel.anchoredPosition = new Vector2(this.JadeBoxDefaultPosition.x, this.JadeBoxDefaultPosition.y - num2);
			this.jadeBoxPanel.sizeDelta = new Vector2(this.JadeBoxDefaultSize.x, this.JadeBoxDefaultSize.y + num2);
			for (int i = 0; i < num; i++)
			{
				JadeBoxWidget jadeBoxWidget = Object.Instantiate<JadeBoxWidget>(this.jadeBoxTemplate, this.jadeBoxContent);
				jadeBoxWidget.gameObject.SetActive(true);
				this._jadeBoxWidgets.Add(jadeBoxWidget);
			}
			SystemBoard panel = UiManager.GetPanel<SystemBoard>();
			RectTransform exhibitPanel = panel.exhibitPanel;
			Vector2 exhibitPanelShortSize = panel.ExhibitPanelShortSize;
			if (jadeBoxes.Count == 0)
			{
				this.JadeBoxHint.gameObject.SetActive(false);
				exhibitPanel.sizeDelta = new Vector2(exhibitPanelShortSize.x + 100f, exhibitPanelShortSize.y);
				this._jadeBoxWidgets[0].SetJadeBox(null);
				return;
			}
			this.JadeBoxHint.gameObject.SetActive(true);
			exhibitPanel.sizeDelta = exhibitPanelShortSize;
			foreach (ValueTuple<int, JadeBox> valueTuple in jadeBoxes.WithIndices<JadeBox>())
			{
				int item = valueTuple.Item1;
				JadeBox item2 = valueTuple.Item2;
				this._jadeBoxWidgets[item].SetJadeBox(item2);
			}
		}

		// Token: 0x06000B69 RID: 2921 RVA: 0x0003B768 File Offset: 0x00039968
		private void ClearJadeBoxWidgets()
		{
			this._jadeBoxWidgets.Clear();
			foreach (object obj in this.jadeBoxContent)
			{
				Object.Destroy(((Transform)obj).gameObject);
			}
		}

		// Token: 0x040008E1 RID: 2273
		[SerializeField]
		private BgmHint bgmHint;

		// Token: 0x040008E2 RID: 2274
		[SerializeField]
		private RectTransform root;

		// Token: 0x040008E3 RID: 2275
		[SerializeField]
		private RectTransform messageTemplate;

		// Token: 0x040008E4 RID: 2276
		[SerializeField]
		private RectTransform achievementHintWidget;

		// Token: 0x040008E5 RID: 2277
		private float _messageHeight;

		// Token: 0x040008E6 RID: 2278
		private readonly Queue<string> _achievementQueue = new Queue<string>();

		// Token: 0x040008E7 RID: 2279
		private readonly List<RectTransform> _messages = new List<RectTransform>();

		// Token: 0x040008E8 RID: 2280
		private float hintDelay = 0.3f;

		// Token: 0x040008E9 RID: 2281
		private int maxHints = 5;

		// Token: 0x040008EA RID: 2282
		private bool _isShowingHints;

		// Token: 0x040008EB RID: 2283
		private Coroutine _showHintsCoroutine;

		// Token: 0x040008EC RID: 2284
		private int currentHint;

		// Token: 0x040008ED RID: 2285
		[Header("JadeBox")]
		public RectTransform jadeBoxPanel;

		// Token: 0x040008EE RID: 2286
		[SerializeField]
		private Transform jadeBoxContent;

		// Token: 0x040008EF RID: 2287
		[SerializeField]
		private JadeBoxWidget jadeBoxTemplate;

		// Token: 0x040008F0 RID: 2288
		private readonly List<JadeBoxWidget> _jadeBoxWidgets = new List<JadeBoxWidget>();

		// Token: 0x040008F1 RID: 2289
		private const float JadeBoxSpacing = 220f;

		// Token: 0x040008F4 RID: 2292
		private bool _showJadeBoxPanel;
	}
}
