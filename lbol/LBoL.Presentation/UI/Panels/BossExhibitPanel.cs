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
	public class BossExhibitPanel : UiPanel<Exhibit[]>
	{
		public override PanelLayer Layer
		{
			get
			{
				return PanelLayer.Bottom;
			}
		}
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
		protected override void OnHided()
		{
			this._exhibitList.Clear();
			foreach (Transform transform in this.contentList)
			{
				Object.Destroy(transform.GetComponentInChildren<ExhibitWidget>().gameObject);
				transform.gameObject.SetActive(false);
			}
		}
		private bool IsExhibitNonInitialColor(Exhibit exhibit)
		{
			ManaGroup mana = exhibit.BaseMana.Value;
			return mana != ManaGroup.Empty && Enumerable.Any<ManaColor>(ManaColors.TrivialColors, (ManaColor color) => mana.HasColor(color) && !this.GameRun.Player.Config.InitialMana.HasColor(color));
		}
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
		public Vector3 GetRewardWorldPosition(int index)
		{
			return this.contentList[index].position;
		}
		public void OnPointerEnterRollCardTips()
		{
			this.rollCardsExplainContent.DOKill(false);
			this.rollCardsExplainContent.DOFade(1f, 0.2f).From(0f, true, false);
		}
		public void OnPointerRollCardExitTips()
		{
			this.rollCardsExplainContent.DOKill(false);
			this.rollCardsExplainContent.DOFade(0f, 0.2f).From(1f, true, false);
		}
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
		[SerializeField]
		private ExhibitWidget exhibitPrefab;
		[SerializeField]
		private List<Transform> contentList;
		[SerializeField]
		private Image nonInitialColorHint;
		[SerializeField]
		private CanvasGroup rollCardsExplainContent;
		private readonly List<ExhibitWidget> _exhibitList = new List<ExhibitWidget>();
		private bool _revealedNonInitialColorHint;
		private ExhibitWidget _lastInteractExhibit;
	}
}
