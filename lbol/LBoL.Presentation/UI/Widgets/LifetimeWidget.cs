using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LBoL.Core;
using LBoL.Core.Units;
using LBoL.Presentation.UI.ExtraWidgets;
using LBoL.Presentation.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x0200005F RID: 95
	public class LifetimeWidget : MonoBehaviour
	{
		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x0600053E RID: 1342 RVA: 0x000166A4 File Offset: 0x000148A4
		private float TargetHeight
		{
			get
			{
				return (float)(this.isOnClickExpand ? (95 + 60 * (this._order + 1)) : 90);
			}
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x0600053F RID: 1343 RVA: 0x000166C1 File Offset: 0x000148C1
		private float TargetArrowRectZRotation
		{
			get
			{
				if (!this.isOnClickExpand)
				{
					return -90f;
				}
				return -180f;
			}
		}

		// Token: 0x06000540 RID: 1344 RVA: 0x000166D8 File Offset: 0x000148D8
		public void UpdateCharacterStatus(Dictionary<string, string> status)
		{
			this.nameTmp.text = ("Lifetime." + this._nameKey).Localize(true) + " " + (this._noDataHint ? "<sprite=\"TextIcon\" name=\"Info\">" : "");
			foreach (KeyValuePair<string, ValueTuple<RectTransform, CharacterLifetimeWidget>> keyValuePair in this._characterLine)
			{
				if (keyValuePair.Value.Item2.isOld)
				{
					keyValuePair.Value.Item2.SetValue(Singleton<GameMaster>.Instance.CurrentProfile.BluePoint.ToString());
					keyValuePair.Value.Item2.Refresh();
				}
				else
				{
					string text;
					if (status.TryGetValue(keyValuePair.Value.Item2.Chara.Id, ref text))
					{
						keyValuePair.Value.Item2.SetValue(text);
					}
					else
					{
						keyValuePair.Value.Item2.SetValue("-");
					}
					keyValuePair.Value.Item2.Refresh();
				}
			}
		}

		// Token: 0x06000541 RID: 1345 RVA: 0x00016810 File Offset: 0x00014A10
		public void Awake()
		{
			this.mainButton.onClick.AddListener(new UnityAction(this.HandleButtonClick));
			this._parentRect = base.transform.parent.GetComponentInParent<RectTransform>();
			foreach (PlayerUnit playerUnit in Enumerable.OrderBy<PlayerUnit, int>(Library.GetSelectablePlayers(), (PlayerUnit player) => player.Config.Order))
			{
				RectTransform rectTransform = Object.Instantiate<RectTransform>(this.characterLifetimeTemplate, this.characterList);
				rectTransform.gameObject.SetActive(true);
				CharacterLifetimeWidget component = rectTransform.GetComponent<CharacterLifetimeWidget>();
				component.SetTitle(playerUnit, false);
				this._characterLine.Add(playerUnit.Id, new ValueTuple<RectTransform, CharacterLifetimeWidget>(rectTransform, component));
				component.Order = this._order;
				this._order++;
				rectTransform.gameObject.GetOrAddComponent<CanvasGroup>().alpha = 0f;
			}
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x00016924 File Offset: 0x00014B24
		public void HandleButtonClick()
		{
			if (!this.isMotioning)
			{
				this.mainButton.enabled = false;
				AudioManager.PlayUi(this.isOnClickExpand ? "MapRoll0" : "MapRoll1", false);
				this.isMotioning = true;
				RectTransform component = base.GetComponent<RectTransform>();
				this.arrowRect.DORotate(new Vector3(0f, 0f, this.TargetArrowRectZRotation), 0.1375f, RotateMode.Fast).SetEase(Ease.OutCubic).SetDelay(this.isOnClickExpand ? 0f : 0.165f);
				bool currentExpand = this.isOnClickExpand;
				using (Dictionary<string, ValueTuple<RectTransform, CharacterLifetimeWidget>>.Enumerator enumerator = this._characterLine.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, ValueTuple<RectTransform, CharacterLifetimeWidget>> pair = enumerator.Current;
						CanvasGroup component2 = pair.Value.Item1.GetComponent<CanvasGroup>();
						float num = 0.04125f;
						int order = pair.Value.Item2.Order;
						float num2 = (this.isOnClickExpand ? ((float)order * num) : (0.28599998f - (float)order * num));
						if (currentExpand)
						{
							pair.Value.Item1.gameObject.SetActive(true);
						}
						DOTween.Sequence().Join(component2.DOFade(this.isOnClickExpand ? 1f : 0f, 0.275f).From((!this.isOnClickExpand) ? 1f : 0f, true, false).SetDelay(num2)
							.SetEase(Ease.OutCubic)).Join(pair.Value.Item1.DOLocalMoveX(this.isOnClickExpand ? 0f : (50f * (float)((order % 2 == 0) ? (-1) : 1)), 0.275f, false).From((!this.isOnClickExpand) ? 0f : (50f * (float)((order % 2 == 0) ? (-1) : 1)), true, false).SetEase(Ease.OutCubic))
							.OnComplete(delegate
							{
								if (!currentExpand)
								{
									pair.Value.Item1.gameObject.SetActive(false);
								}
							});
					}
				}
				VerticalLayoutGroup grid = base.GetComponentInParent<VerticalLayoutGroup>();
				MuseumPanel museum = base.GetComponentInParent<MuseumPanel>();
				grid.CalculateLayoutInputVertical();
				grid.SetLayoutVertical();
				DOTween.Sequence().Append(component.DOSizeDelta(new Vector2(component.sizeDelta.x, this.TargetHeight), 0.55f * (this.isOnClickExpand ? 0.8f : 1f), false).SetDelay(this.isOnClickExpand ? 0f : 0.165f).SetEase(Ease.OutCubic)).OnUpdate(delegate
				{
					grid.CalculateLayoutInputVertical();
					grid.SetLayoutVertical();
					museum.UpdateLifetimeGrid();
				})
					.OnComplete(delegate
					{
						this.isMotioning = false;
						this.isOnClickExpand = !this.isOnClickExpand;
						this.mainButton.enabled = true;
					});
			}
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x00016C2C File Offset: 0x00014E2C
		public void Initialize(string nameKey, bool noDataHint = false, bool oldDataHint = false)
		{
			this._nameKey = nameKey;
			this._noDataHint = noDataHint;
			this.nameTmp.text = ("Lifetime." + nameKey).Localize(true) + " " + (noDataHint ? "<sprite=\"TextIcon\" name=\"Info\">" : "");
			if (noDataHint)
			{
				SimpleTooltipSource.CreateWithGeneralKey(base.GetComponentInChildren<Button>().gameObject, "Lifetime.NoData", "Lifetime.NoDataHint");
			}
			if (oldDataHint)
			{
				RectTransform rectTransform = Object.Instantiate<RectTransform>(this.characterLifetimeTemplate, this.characterList);
				rectTransform.gameObject.SetActive(true);
				CharacterLifetimeWidget component = rectTransform.GetComponent<CharacterLifetimeWidget>();
				component.SetTitle(null, true);
				component.SetValue(Singleton<GameMaster>.Instance.CurrentProfile.BluePoint.ToString());
				this._characterLine.Add("OldData", new ValueTuple<RectTransform, CharacterLifetimeWidget>(rectTransform, component));
				component.Order = this._order;
				this._order++;
				rectTransform.gameObject.GetOrAddComponent<CanvasGroup>().alpha = 0f;
				SimpleTooltipSource.CreateWithGeneralKey(rectTransform.gameObject, "Lifetime.OldData", "Lifetime.OldDataHint");
				rectTransform.gameObject.SetActive(false);
			}
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x00016D4F File Offset: 0x00014F4F
		public void SetTotalValue(int intValue)
		{
			this.valueTmp.text = intValue.ToString();
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x00016D63 File Offset: 0x00014F63
		public void SetTotalValue(string valueText)
		{
			this.valueTmp.text = valueText;
		}

		// Token: 0x04000300 RID: 768
		[SerializeField]
		private TextMeshProUGUI nameTmp;

		// Token: 0x04000301 RID: 769
		[SerializeField]
		private TextMeshProUGUI valueTmp;

		// Token: 0x04000302 RID: 770
		[SerializeField]
		private Button mainButton;

		// Token: 0x04000303 RID: 771
		[SerializeField]
		private RectTransform arrowRect;

		// Token: 0x04000304 RID: 772
		[SerializeField]
		private RectTransform characterList;

		// Token: 0x04000305 RID: 773
		[SerializeField]
		private RectTransform characterLifetimeTemplate;

		// Token: 0x04000306 RID: 774
		private bool isOnClickExpand = true;

		// Token: 0x04000307 RID: 775
		private bool isMotioning;

		// Token: 0x04000308 RID: 776
		private const float FADE_TIME = 0.55f;

		// Token: 0x04000309 RID: 777
		[TupleElementNames(new string[] { "rect", "widget" })]
		private Dictionary<string, ValueTuple<RectTransform, CharacterLifetimeWidget>> _characterLine = new Dictionary<string, ValueTuple<RectTransform, CharacterLifetimeWidget>>();

		// Token: 0x0400030A RID: 778
		private int _order;

		// Token: 0x0400030B RID: 779
		private RectTransform _parentRect;

		// Token: 0x0400030C RID: 780
		private string _nameKey;

		// Token: 0x0400030D RID: 781
		private bool _noDataHint;
	}
}
