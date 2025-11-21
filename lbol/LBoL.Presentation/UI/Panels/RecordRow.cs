using System;
using System.Linq;
using DG.Tweening;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Helpers;
using LBoL.Core.SaveData;
using LBoL.EntityLib.Exhibits;
using LBoL.Presentation.I10N;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LBoL.Presentation.UI.Panels
{
	// Token: 0x020000AA RID: 170
	[DisallowMultipleComponent]
	public sealed class RecordRow : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		// Token: 0x17000177 RID: 375
		// (get) Token: 0x0600096C RID: 2412 RVA: 0x00030333 File Offset: 0x0002E533
		// (set) Token: 0x0600096D RID: 2413 RVA: 0x0003033B File Offset: 0x0002E53B
		public GameRunRecordSaveData Record { get; private set; }

		// Token: 0x0600096E RID: 2414 RVA: 0x00030344 File Offset: 0x0002E544
		private void Awake()
		{
			this.cover.color = Color.black.WithA(0.5f);
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x00030360 File Offset: 0x0002E560
		private void OnDestroy()
		{
			this.cover.DOKill(false);
			this.root.DOKill(false);
		}

		// Token: 0x06000970 RID: 2416 RVA: 0x0003037C File Offset: 0x0002E57C
		public void Set(Sprite avatar, GameRunRecordSaveData record)
		{
			this.Record = record;
			if (avatar != null)
			{
				this.avatarImage.sprite = avatar;
			}
			this.difficultyText.text = ((record.Puzzles != PuzzleFlag.None) ? string.Format("{0} ({1})", record.Difficulty, Enumerable.Count<PuzzleFlag>(PuzzleFlags.EnumerateComponents(record.Puzzles))) : record.Difficulty.ToString());
			DateTime dateTime;
			this.timestampText.text = (Utils.TryParseIso8601Timestamp(record.SaveTimestamp, out dateTime) ? dateTime.ToLocalTime().ToString(L10nManager.Info.Culture) : "<Error>");
			TextMeshProUGUI textMeshProUGUI = this.gameResultText;
			string text;
			switch (record.ResultType)
			{
			case GameResultType.Failure:
				text = UiUtils.WrapByColor("GameResult.Lose".Localize(true), GlobalConfig.GameResultFail);
				break;
			case GameResultType.NormalEnd:
				text = UiUtils.WrapByColor("GameResult.Win".Localize(true), GlobalConfig.GameResultNormal);
				break;
			case GameResultType.TrueEndFail:
				text = UiUtils.WrapByColor("GameResult.Win".Localize(true), GlobalConfig.GameResultNormal);
				break;
			case GameResultType.TrueEnd:
				text = UiUtils.WrapByColor("GameResult.TrueEnd".Localize(true), GlobalConfig.GameResultTrue);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			textMeshProUGUI.text = text;
			if (record.Exhibits.Length != 0)
			{
				this.exhibitIcon.gameObject.SetActive(true);
				Exhibit exhibit = Library.TryCreateExhibit(record.Exhibits[0]);
				if (exhibit == null)
				{
					exhibit = Library.CreateExhibit<KongZhanpinhe>();
				}
				this.exhibitIcon.sprite = ResourcesHelper.TryGetSprite<Exhibit>(exhibit.Id);
				return;
			}
			this.exhibitIcon.gameObject.SetActive(false);
		}

		// Token: 0x06000971 RID: 2417 RVA: 0x00030521 File Offset: 0x0002E721
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.cover.DOKill(false);
			this.cover.DOFade(0f, 0.2f);
		}

		// Token: 0x06000972 RID: 2418 RVA: 0x00030546 File Offset: 0x0002E746
		public void OnPointerExit(PointerEventData eventData)
		{
			this.cover.DOKill(false);
			this.cover.DOFade(0.5f, 0.2f);
		}

		// Token: 0x06000973 RID: 2419 RVA: 0x0003056B File Offset: 0x0002E76B
		public void OnPointerClick(PointerEventData eventData)
		{
			Action click = this.Click;
			if (click == null)
			{
				return;
			}
			click.Invoke();
		}

		// Token: 0x06000974 RID: 2420 RVA: 0x00030580 File Offset: 0x0002E780
		public void SetSelected(bool selected, bool tween)
		{
			this.selectedIndicator.SetActive(selected);
			this.cover.gameObject.SetActive(!selected);
			this.root.DOKill(false);
			if (tween)
			{
				this.root.DOAnchorPosX(selected ? 0f : this.unselectedDeltaX, 0.2f, false);
				return;
			}
			this.root.anchoredPosition = this.root.anchoredPosition.WithX(selected ? 0f : this.unselectedDeltaX);
		}

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06000975 RID: 2421 RVA: 0x0003060C File Offset: 0x0002E80C
		// (remove) Token: 0x06000976 RID: 2422 RVA: 0x00030644 File Offset: 0x0002E844
		public event Action Click;

		// Token: 0x040006EF RID: 1775
		[SerializeField]
		private Image avatarImage;

		// Token: 0x040006F0 RID: 1776
		[SerializeField]
		private TextMeshProUGUI gameResultText;

		// Token: 0x040006F1 RID: 1777
		[SerializeField]
		private TextMeshProUGUI difficultyText;

		// Token: 0x040006F2 RID: 1778
		[SerializeField]
		private TextMeshProUGUI timestampText;

		// Token: 0x040006F3 RID: 1779
		[SerializeField]
		private GameObject selectedIndicator;

		// Token: 0x040006F4 RID: 1780
		[SerializeField]
		private Image exhibitIcon;

		// Token: 0x040006F5 RID: 1781
		[SerializeField]
		private RectTransform root;

		// Token: 0x040006F6 RID: 1782
		[SerializeField]
		private Image cover;

		// Token: 0x040006F7 RID: 1783
		[SerializeField]
		private float unselectedDeltaX;
	}
}
