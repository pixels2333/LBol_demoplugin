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
	// Token: 0x02000087 RID: 135
	public class BattleHintPanel : UiPanel, IInputActionHandler
	{
		// Token: 0x060006B6 RID: 1718 RVA: 0x0001D858 File Offset: 0x0001BA58
		private void Awake()
		{
			this._detailedGroups = new CanvasGroup[] { this.basicBattleGroup, this.manaGroup, this.manaPanelGroup, this.blockShieldGroup, this.intentionGroup, this.ultimateSkillGroup };
			this._briefGroups = new CanvasGroup[] { this.manaGroup, this.manaPanelGroup, this.blockShieldGroup, this.ultimateSkillGroup };
			this.showPrevButton.onClick.AddListener(new UnityAction(this.ShowPrevPage));
			this.showNextButton.onClick.AddListener(new UnityAction(this.ShowNextPage));
		}

		// Token: 0x060006B7 RID: 1719 RVA: 0x0001D910 File Offset: 0x0001BB10
		public override void OnLocaleChanged()
		{
			SimpleTooltipSource.CreateWithTooltipKey(this.detailedButton.gameObject, "DetailedHint").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.detailedButton.onClick.AddListener(new UnityAction(this.OnDetailedClick));
			SimpleTooltipSource.CreateWithTooltipKey(this.briefButton.gameObject, "BriefHint").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.briefButton.onClick.AddListener(new UnityAction(this.OnBriefClick));
			SimpleTooltipSource.CreateWithTooltipKey(this.dontShowButton.gameObject, "DontShowHint").WithPosition(TooltipDirection.Top, TooltipAlignment.Max);
			this.dontShowButton.onClick.AddListener(new UnityAction(this.OnDontShowClick));
		}

		// Token: 0x060006B8 RID: 1720 RVA: 0x0001D9C8 File Offset: 0x0001BBC8
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

		// Token: 0x060006B9 RID: 1721 RVA: 0x0001DA35 File Offset: 0x0001BC35
		private void OnDetailedClick()
		{
			GameMaster.HintLevel = HintLevel.Detailed;
			this._detailed = true;
			this.detailedButton.interactable = false;
			this.briefButton.interactable = false;
			this.dontShowButton.interactable = false;
			this.ShowNextPage();
		}

		// Token: 0x060006BA RID: 1722 RVA: 0x0001DA6E File Offset: 0x0001BC6E
		private void OnBriefClick()
		{
			GameMaster.HintLevel = HintLevel.Brief;
			this._detailed = false;
			this.detailedButton.interactable = false;
			this.briefButton.interactable = false;
			this.dontShowButton.interactable = false;
			this.ShowNextPage();
		}

		// Token: 0x060006BB RID: 1723 RVA: 0x0001DAA7 File Offset: 0x0001BCA7
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

		// Token: 0x060006BC RID: 1724 RVA: 0x0001DADC File Offset: 0x0001BCDC
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

		// Token: 0x060006BD RID: 1725 RVA: 0x0001DCA0 File Offset: 0x0001BEA0
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

		// Token: 0x060006BE RID: 1726 RVA: 0x0001DE7C File Offset: 0x0001C07C
		public void StartBattleHint()
		{
			this.bg.DOFade(0.95f, 0.2f).SetUpdate(true);
			this.introGroup.gameObject.SetActive(true);
			this.introGroup.DOFade(1f, 0.2f).From(0f, true, false).SetUpdate(true);
			this.detailedButton.interactable = true;
			this.briefButton.interactable = true;
			this.dontShowButton.interactable = true;
		}

		// Token: 0x0400044A RID: 1098
		[SerializeField]
		private RawImage bg;

		// Token: 0x0400044B RID: 1099
		[SerializeField]
		private CanvasGroup introGroup;

		// Token: 0x0400044C RID: 1100
		[SerializeField]
		private CanvasGroup basicBattleGroup;

		// Token: 0x0400044D RID: 1101
		[SerializeField]
		private CanvasGroup manaGroup;

		// Token: 0x0400044E RID: 1102
		[SerializeField]
		private CanvasGroup manaPanelGroup;

		// Token: 0x0400044F RID: 1103
		[SerializeField]
		private CanvasGroup blockShieldGroup;

		// Token: 0x04000450 RID: 1104
		[SerializeField]
		private CanvasGroup intentionGroup;

		// Token: 0x04000451 RID: 1105
		[SerializeField]
		private CanvasGroup ultimateSkillGroup;

		// Token: 0x04000452 RID: 1106
		[SerializeField]
		private GameObject navigationRoot;

		// Token: 0x04000453 RID: 1107
		[SerializeField]
		private TextMeshProUGUI pageInfoText;

		// Token: 0x04000454 RID: 1108
		[SerializeField]
		private Button showPrevButton;

		// Token: 0x04000455 RID: 1109
		[SerializeField]
		private Button showNextButton;

		// Token: 0x04000456 RID: 1110
		[SerializeField]
		private Button detailedButton;

		// Token: 0x04000457 RID: 1111
		[SerializeField]
		private Button briefButton;

		// Token: 0x04000458 RID: 1112
		[SerializeField]
		private Button dontShowButton;

		// Token: 0x04000459 RID: 1113
		private CanvasGroup[] _detailedGroups;

		// Token: 0x0400045A RID: 1114
		private CanvasGroup[] _briefGroups;

		// Token: 0x0400045B RID: 1115
		private bool _detailed;

		// Token: 0x0400045C RID: 1116
		private int _currentGroupIndex;

		// Token: 0x0400045D RID: 1117
		private const float BgFinalAlpha = 0.95f;

		// Token: 0x0400045E RID: 1118
		private const float PageTweenDeltaX = 500f;

		// Token: 0x0400045F RID: 1119
		private const float PageTweenDuration = 0.2f;
	}
}
