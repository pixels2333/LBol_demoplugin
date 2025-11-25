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
	[DisallowMultipleComponent]
	public sealed class RecordRow : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public GameRunRecordSaveData Record { get; private set; }
		private void Awake()
		{
			this.cover.color = Color.black.WithA(0.5f);
		}
		private void OnDestroy()
		{
			this.cover.DOKill(false);
			this.root.DOKill(false);
		}
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
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.cover.DOKill(false);
			this.cover.DOFade(0f, 0.2f);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.cover.DOKill(false);
			this.cover.DOFade(0.5f, 0.2f);
		}
		public void OnPointerClick(PointerEventData eventData)
		{
			Action click = this.Click;
			if (click == null)
			{
				return;
			}
			click.Invoke();
		}
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
		public event Action Click;
		[SerializeField]
		private Image avatarImage;
		[SerializeField]
		private TextMeshProUGUI gameResultText;
		[SerializeField]
		private TextMeshProUGUI difficultyText;
		[SerializeField]
		private TextMeshProUGUI timestampText;
		[SerializeField]
		private GameObject selectedIndicator;
		[SerializeField]
		private Image exhibitIcon;
		[SerializeField]
		private RectTransform root;
		[SerializeField]
		private Image cover;
		[SerializeField]
		private float unselectedDeltaX;
	}
}
