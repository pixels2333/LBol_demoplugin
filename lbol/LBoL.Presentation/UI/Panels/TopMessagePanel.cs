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
	public sealed class TopMessagePanel : UiPanel
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Topmost;
			}
		}
		private void ShowBgmHint(BgmConfig config)
		{
			this.bgmHint.ShowHint(config);
		}
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
		private void OnEnable()
		{
			AudioManager.BgmChanged += new Action<BgmConfig>(this.ShowBgmHint);
		}
		private void OnDisable()
		{
			AudioManager.BgmChanged -= new Action<BgmConfig>(this.ShowBgmHint);
		}
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
		public void UnlockAchievement(string key)
		{
			this._achievementQueue.Enqueue(key);
			if (!this._isShowingHints)
			{
				this._isShowingHints = true;
				this._showHintsCoroutine = base.StartCoroutine(this.ShowAchievementHints());
			}
		}
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
		public void ClearAllHints()
		{
			if (this._showHintsCoroutine != null)
			{
				base.StopCoroutine(this._showHintsCoroutine);
			}
			this._achievementQueue.Clear();
			this._isShowingHints = false;
		}
		private RectTransform JadeBoxHint
		{
			get
			{
				return UiManager.GetPanel<SystemBoard>().jadeBoxHint;
			}
		}
		private Vector2 JadeBoxDefaultPosition { get; set; }
		private Vector2 JadeBoxDefaultSize { get; set; }
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
		private void ClearJadeBoxWidgets()
		{
			this._jadeBoxWidgets.Clear();
			foreach (object obj in this.jadeBoxContent)
			{
				Object.Destroy(((Transform)obj).gameObject);
			}
		}
		[SerializeField]
		private BgmHint bgmHint;
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private RectTransform messageTemplate;
		[SerializeField]
		private RectTransform achievementHintWidget;
		private float _messageHeight;
		private readonly Queue<string> _achievementQueue = new Queue<string>();
		private readonly List<RectTransform> _messages = new List<RectTransform>();
		private float hintDelay = 0.3f;
		private int maxHints = 5;
		private bool _isShowingHints;
		private Coroutine _showHintsCoroutine;
		private int currentHint;
		[Header("JadeBox")]
		public RectTransform jadeBoxPanel;
		[SerializeField]
		private Transform jadeBoxContent;
		[SerializeField]
		private JadeBoxWidget jadeBoxTemplate;
		private readonly List<JadeBoxWidget> _jadeBoxWidgets = new List<JadeBoxWidget>();
		private const float JadeBoxSpacing = 220f;
		private bool _showJadeBoxPanel;
	}
}
