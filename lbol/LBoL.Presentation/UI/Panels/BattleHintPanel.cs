using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base.Extensions;
using LBoL.Core.SaveData;
using LBoL.Presentation.UI.Dialogs;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Panels
{
	public class BattleHintPanel : UiPanel, IInputActionHandler
	{
		private void Awake()
		{
			this._detailedGroups = new CanvasGroup[] { this.basicBattleGroup, this.manaGroup, this.manaPanelGroup, this.blockShieldGroup, this.intentionGroup, this.ultimateSkillGroup };
			this._briefGroups = new CanvasGroup[] { this.manaGroup, this.manaPanelGroup, this.blockShieldGroup, this.ultimateSkillGroup };
			this.showPrevButton.onClick.AddListener(new UnityAction(this.ShowPrevPage));
			this.showNextButton.onClick.AddListener(new UnityAction(this.ShowNextPage));
		}
		public override void OnLocaleChanged()
		{
			SimpleTooltipSource.CreateWithTooltipKey(this.detailedButton.gameObject, "DetailedHint").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.detailedButton.onClick.AddListener(new UnityAction(this.OnDetailedClick));
			SimpleTooltipSource.CreateWithTooltipKey(this.briefButton.gameObject, "BriefHint").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.briefButton.onClick.AddListener(new UnityAction(this.OnBriefClick));
			SimpleTooltipSource.CreateWithTooltipKey(this.dontShowButton.gameObject, "DontShowHint").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.dontShowButton.onClick.AddListener(new UnityAction(this.OnDontShowClick));
		}
		protected override void OnShowing()
		{
			this.bg.color = Color.black.WithA(0f);
			this.introGroup.alpha = 0f;
			this.navigationRoot.SetActive(false);
			CanvasGroup[] detailedGroups = this._detailedGroups;
			for (int i = 0; i < detailedGroups.Length; i++)
			{
				detailedGroups[i].gameObject.SetActive(false);
			}
			this._currentGroupIndex = -1;
		}
		private void OnDetailedClick()
		{
			GameMaster.HintLevel = HintLevel.Detailed;
			this._detailed = true;
			this.detailedButton.interactable = false;
			this.briefButton.interactable = false;
			this.dontShowButton.interactable = false;
			this.ShowNextPage();
		}
		private void OnBriefClick()
		{
			GameMaster.HintLevel = HintLevel.Brief;
			this._detailed = false;
			this.detailedButton.interactable = false;
			this.briefButton.interactable = false;
			this.dontShowButton.interactable = false;
			this.ShowNextPage();
		}
		private void OnDontShowClick()
		{
			UiManager.GetDialog<MessageDialog>().Show(new MessageContent
			{
				TextKey = "DontShowHintWarning",
				Buttons = DialogButtons.ConfirmCancel,
				OnConfirm = delegate
				{
					GameMaster.HintLevel = HintLevel.DontShow;
					base.Hide();
				}
			});
		}
		private void ShowNextPage()
		{
			CanvasGroup[] array = (this._detailed ? this._detailedGroups : this._briefGroups);
			if (this._currentGroupIndex == array.Length - 1)
			{
				UiManager.GetDialog<MessageDialog>().Show(new MessageContent
				{
					TextKey = "CloseBattleHint",
					Buttons = DialogButtons.ConfirmCancel,
					OnConfirm = delegate
					{
						base.Hide();
					}
				});
				return;
			}
			if (this._currentGroupIndex < 0)
			{
				this.introGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(delegate
				{
					this.introGroup.gameObject.SetActive(false);
				});
			}
			else
			{
				CanvasGroup current = array[this._currentGroupIndex];
				DOTween.Sequence().Join(current.DOFade(0f, 0.2f)).Join(current.transform.DOLocalMoveX(-500f, 0.2f, false).From(0f, true, false))
					.SetUpdate(true)
					.OnComplete(delegate
					{
						current.gameObject.SetActive(false);
					});
			}
			this._currentGroupIndex++;
			this.navigationRoot.SetActive(true);
			this.pageInfoText.text = string.Format("{0}/{1}", this._currentGroupIndex + 1, array.Length);
			CanvasGroup canvasGroup = array[this._currentGroupIndex];
			canvasGroup.gameObject.SetActive(true);
			canvasGroup.DOFade(1f, 0.2f).From(0f, true, false).SetUpdate(true);
			canvasGroup.transform.DOLocalMoveX(0f, 0.2f, false).From(500f, true, false).SetUpdate(true);
			this.showPrevButton.interactable = true;
		}
		private void ShowPrevPage()
		{
			CanvasGroup[] array = (this._detailed ? this._detailedGroups : this._briefGroups);
			if (this._currentGroupIndex >= 0)
			{
				CanvasGroup current = array[this._currentGroupIndex];
				DOTween.Sequence().Join(current.DOFade(0f, 0.2f)).Join(current.transform.DOLocalMoveX(500f, 0.2f, false).From(0f, true, false))
					.SetUpdate(true)
					.OnComplete(delegate
					{
						current.gameObject.SetActive(false);
					});
				this._currentGroupIndex--;
				if (this._currentGroupIndex < 0)
				{
					this.navigationRoot.SetActive(false);
					this.introGroup.gameObject.SetActive(true);
					this.introGroup.DOFade(1f, 0.2f).From(0f, true, false).SetUpdate(true)
						.OnComplete(delegate
						{
							this.detailedButton.interactable = true;
							this.briefButton.interactable = true;
							this.dontShowButton.interactable = true;
						});
					this.showPrevButton.interactable = this._currentGroupIndex > 0;
				}
				else
				{
					this.pageInfoText.text = string.Format("{0}/{1}", this._currentGroupIndex + 1, array.Length);
					CanvasGroup canvasGroup = array[this._currentGroupIndex];
					canvasGroup.gameObject.SetActive(true);
					canvasGroup.DOFade(1f, 0.2f).From(0f, true, false).SetUpdate(true);
					canvasGroup.transform.DOLocalMoveX(0f, 0.2f, false).From(-500f, true, false).SetUpdate(true);
				}
				this.showNextButton.interactable = true;
				return;
			}
			Debug.LogError(string.Format("Cannot show prev page while currentGroupIndex = {0}", this._currentGroupIndex));
		}
		public void StartBattleHint()
		{
			this.bg.DOFade(0.95f, 0.2f).SetUpdate(true);
			this.introGroup.gameObject.SetActive(true);
			this.introGroup.DOFade(1f, 0.2f).From(0f, true, false).SetUpdate(true);
			this.detailedButton.interactable = true;
			this.briefButton.interactable = true;
			this.dontShowButton.interactable = true;
		}
		[SerializeField]
		private RawImage bg;
		[SerializeField]
		private CanvasGroup introGroup;
		[SerializeField]
		private CanvasGroup basicBattleGroup;
		[SerializeField]
		private CanvasGroup manaGroup;
		[SerializeField]
		private CanvasGroup manaPanelGroup;
		[SerializeField]
		private CanvasGroup blockShieldGroup;
		[SerializeField]
		private CanvasGroup intentionGroup;
		[SerializeField]
		private CanvasGroup ultimateSkillGroup;
		[SerializeField]
		private GameObject navigationRoot;
		[SerializeField]
		private TextMeshProUGUI pageInfoText;
		[SerializeField]
		private Button showPrevButton;
		[SerializeField]
		private Button showNextButton;
		[SerializeField]
		private Button detailedButton;
		[SerializeField]
		private Button briefButton;
		[SerializeField]
		private Button dontShowButton;
		private CanvasGroup[] _detailedGroups;
		private CanvasGroup[] _briefGroups;
		private bool _detailed;
		private int _currentGroupIndex;
		private const float BgFinalAlpha = 0.95f;
		private const float PageTweenDeltaX = 500f;
		private const float PageTweenDuration = 0.2f;
	}
}
