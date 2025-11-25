using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LBoL.Presentation.UI.Widgets
{
	public class RewardWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public Vector3? ApproachingPosition { get; set; }
		public CardWidget ToolCardWidget
		{
			get
			{
				return this.toolCard;
			}
		}
		private void Awake()
		{
			this._rectTransform = base.GetComponent<RectTransform>();
			this.button.onClick.AddListener(delegate
			{
				Action click = this.Click;
				if (click == null)
				{
					return;
				}
				click.Invoke();
			});
			this.toolCard.GetComponent<ShowingCard>().SetScale(1f);
		}
		private void Update()
		{
			Vector3? approachingPosition = this.ApproachingPosition;
			if (approachingPosition != null)
			{
				Vector3 valueOrDefault = approachingPosition.GetValueOrDefault();
				if (this._rectTransform.localPosition != valueOrDefault)
				{
					this._rectTransform.localPosition = Vector3.Lerp(this._rectTransform.localPosition, valueOrDefault, 0.2f);
				}
			}
		}
		public StationReward StationReward
		{
			get
			{
				return this._stationReward;
			}
			set
			{
				this._stationReward = value;
				if (value == null)
				{
					return;
				}
				Image image = this.bg;
				RewardWidget.Pair pair2 = Enumerable.FirstOrDefault<RewardWidget.Pair>(this.bgTable, (RewardWidget.Pair pair) => pair.type == value.Type);
				image.sprite = ((pair2 != null) ? pair2.sprite : null);
				this.toolCard.gameObject.SetActive(false);
				switch (value.Type)
				{
				case StationRewardType.Money:
					this.title.text = "Game.Money".Localize(true);
					this.description.text = "Reward.Money".LocalizeFormat(new object[] { value.Money });
					return;
				case StationRewardType.Card:
					this.title.text = "Game.Card".Localize(true);
					this.description.text = "Reward.Card".Localize(true);
					return;
				case StationRewardType.Exhibit:
					this.exhibit.gameObject.SetActive(true);
					this.exhibit.Exhibit = value.Exhibit;
					this.title.text = "Game.Exhibit".Localize(true);
					this.description.text = value.Exhibit.Name;
					return;
				case StationRewardType.Tool:
					this.title.text = "";
					this.description.text = "";
					this.toolCard.Card = Enumerable.FirstOrDefault<Card>(value.Cards);
					this.toolCard.gameObject.SetActive(true);
					return;
				case StationRewardType.RemoveCard:
					this.title.text = "Reward.RemoveCard".Localize(true);
					this.description.text = "Reward.RemoveCardDescription".Localize(true);
					return;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
		}
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.description.color = new Color(0.94f, 0.78f, 0.32f);
		}
		public void OnPointerExit(PointerEventData eventData)
		{
			this.description.color = Color.white;
		}
		public event Action Click;
		[SerializeField]
		private List<RewardWidget.Pair> bgTable;
		[SerializeField]
		private Button button;
		[SerializeField]
		private ExhibitWidget exhibit;
		[SerializeField]
		private Image bg;
		[SerializeField]
		private TextMeshProUGUI title;
		[SerializeField]
		private TextMeshProUGUI description;
		[SerializeField]
		private CardWidget toolCard;
		private RectTransform _rectTransform;
		private StationReward _stationReward;
		[Serializable]
		public class Pair
		{
			public StationRewardType type;
			public Sprite sprite;
		}
	}
}
