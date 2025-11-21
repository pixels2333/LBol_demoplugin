using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Widgets;
using UnityEngine;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x0200008A RID: 138
	public class BossExhibitPanel : UiPanel<Exhibit[]>
	{
		// Token: 0x17000131 RID: 305
		// (get) Token: 0x06000706 RID: 1798 RVA: 0x0002022B File Offset: 0x0001E42B
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}

		// Token: 0x06000707 RID: 1799 RVA: 0x00020230 File Offset: 0x0001E430
		protected override void OnShowing(Exhibit[] exhibits)
		{
			foreach (Transform transform in this.contentList)
			{
				transform.gameObject.SetActive(false);
			}
			foreach (ValueTuple<int, Exhibit> valueTuple in exhibits.WithIndices<Exhibit>())
			{
				int i = valueTuple.Item1;
				Exhibit exhibit = valueTuple.Item2;
				ExhibitWidget exhibitWidget = Object.Instantiate<ExhibitWidget>(this.exhibitPrefab, this.contentList[i]);
				exhibitWidget.transform.localScale = Vector3.one * 1.5f;
				exhibitWidget.Exhibit = exhibit;
				exhibitWidget.name = "Exhibit: " + exhibit.Id;
				exhibitWidget.ShowCounter = false;
				this._exhibitList.Add(exhibitWidget);
				this.contentList[i].GetComponent<CanvasGroup>().alpha = 0f;
				exhibitWidget.ExhibitClicked += delegate
				{
					this.OnClickExhibit(exhibit, i);
				};
			}
			this.nonInitialColorHint.gameObject.SetActive(false);
			this._revealedNonInitialColorHint = false;
			if (GameMaster.ShowBriefHint && GameMaster.ShouldShowHint("NonInitialColor"))
			{
				foreach (ValueTuple<int, ExhibitWidget> valueTuple2 in this._exhibitList.WithIndices<ExhibitWidget>())
				{
					ExhibitWidget item = valueTuple2.Item2;
					if (this.IsExhibitNonInitialColor(item.Exhibit))
					{
						item.ExhibitExit += new Action<ExhibitWidget>(this.RevealColorHint);
					}
				}
			}
			this.rollCardsExplainContent.gameObject.SetActive(true);
			this.rollCardsExplainContent.alpha = 0f;
		}

		// Token: 0x06000708 RID: 1800 RVA: 0x00020444 File Offset: 0x0001E644
		protected override void OnShown()
		{
			foreach (Transform transform in this.contentList)
			{
				GameObject gameObject = transform.gameObject;
				gameObject.SetActive(true);
				transform.GetComponent<CanvasGroup>().DOFade(1f, 0.6f).From(0f, true, false)
					.SetUpdate(true)
					.SetLink(gameObject);
				transform.GetComponentInChildren<Image>().transform.DORotate(new Vector3(0f, 0f, 360f), 5f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear)
					.SetUpdate(true)
					.SetLink(gameObject);
			}
		}

		// Token: 0x06000709 RID: 1801 RVA: 0x00020514 File Offset: 0x0001E714
		protected override void OnHided()
		{
			this._exhibitList.Clear();
			foreach (Transform transform in this.contentList)
			{
				Object.Destroy(transform.GetComponentInChildren<ExhibitWidget>().gameObject);
				transform.gameObject.SetActive(false);
			}
		}

		// Token: 0x0600070A RID: 1802 RVA: 0x00020588 File Offset: 0x0001E788
		private bool IsExhibitNonInitialColor(Exhibit exhibit)
		{
			ManaGroup mana = exhibit.BaseMana.Value;
			return mana != ManaGroup.Empty && Enumerable.Any<ManaColor>(ManaColors.TrivialColors, (ManaColor color) => mana.HasColor(color) && !this.GameRun.Player.Config.InitialMana.HasColor(color));
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x000205E0 File Offset: 0x0001E7E0
		private void OnClickExhibit(Exhibit exhibit, int index)
		{
			if (exhibit.Config.IsSentinel)
			{
				return;
			}
			foreach (Transform transform in this.contentList)
			{
				transform.GetComponentInChildren<Button>().enabled = false;
			}
			foreach (ExhibitWidget exhibitWidget in this._exhibitList)
			{
				exhibitWidget.GetComponent<ExhibitTooltipSource>().enabled = false;
			}
			base.StartCoroutine(this.CoGainExhibit(exhibit, index));
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x00020698 File Offset: 0x0001E898
		private IEnumerator CoGainExhibit(Exhibit exhibit, int index)
		{
			yield return Singleton<GameMaster>.Instance.CurrentGameRun.GainExhibitRunner(exhibit, true, new VisualSourceData
			{
				SourceType = VisualSourceType.BossReward,
				Index = index
			});
			base.Hide();
			yield break;
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x000206B5 File Offset: 0x0001E8B5
		public Vector3 GetRewardWorldPosition(int index)
		{
			return this.contentList[index].position;
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x000206C8 File Offset: 0x0001E8C8
		public void OnPointerEnterRollCardTips()
		{
			this.rollCardsExplainContent.DOKill(false);
			this.rollCardsExplainContent.DOFade(1f, 0.2f).From(0f, true, false);
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x000206F9 File Offset: 0x0001E8F9
		public void OnPointerRollCardExitTips()
		{
			this.rollCardsExplainContent.DOKill(false);
			this.rollCardsExplainContent.DOFade(0f, 0.2f).From(1f, true, false);
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x0002072C File Offset: 0x0001E92C
		private void RevealColorHint(ExhibitWidget exhibit)
		{
			this._lastInteractExhibit = exhibit;
			if (this._revealedNonInitialColorHint)
			{
				return;
			}
			this._revealedNonInitialColorHint = true;
			Image hint = this.nonInitialColorHint;
			if (hint)
			{
				hint.gameObject.SetActive(true);
				hint.DOKill(false);
				hint.transform.DOKill(false);
				hint.DOFade(1f, 0.2f).From(0f, true, false).SetUpdate(true)
					.OnComplete(delegate
					{
						hint.transform.DOScale(1.1f, 0.5f).From(1f, true, false).SetLoops(-1, LoopType.Yoyo)
							.SetUpdate(true);
					});
			}
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x000207D8 File Offset: 0x0001E9D8
		public void OnPointerEnterColorHint()
		{
			Image hint = this.nonInitialColorHint;
			hint.DOKill(false);
			hint.transform.DOKill(false);
			hint.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(delegate
			{
				hint.gameObject.SetActive(false);
			});
			UiManager.GetPanel<HintPanel>().Show(new HintPayload
			{
				HintKey = "NonInitialColor",
				Target = this._lastInteractExhibit.RectTransform
			});
		}

		// Token: 0x04000489 RID: 1161
		[SerializeField]
		private ExhibitWidget exhibitPrefab;

		// Token: 0x0400048A RID: 1162
		[SerializeField]
		private List<Transform> contentList;

		// Token: 0x0400048B RID: 1163
		[SerializeField]
		private Image nonInitialColorHint;

		// Token: 0x0400048C RID: 1164
		[SerializeField]
		private CanvasGroup rollCardsExplainContent;

		// Token: 0x0400048D RID: 1165
		private readonly List<ExhibitWidget> _exhibitList = new List<ExhibitWidget>();

		// Token: 0x0400048E RID: 1166
		private bool _revealedNonInitialColorHint;

		// Token: 0x0400048F RID: 1167
		private ExhibitWidget _lastInteractExhibit;
	}
}
