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
	// Token: 0x0200006D RID: 109
	public class RewardWidget : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		// Token: 0x17000103 RID: 259
		// (get) Token: 0x060005CE RID: 1486 RVA: 0x0001929E File Offset: 0x0001749E
		// (set) Token: 0x060005CF RID: 1487 RVA: 0x000192A6 File Offset: 0x000174A6
		public Vector3? ApproachingPosition { get; set; }

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x060005D0 RID: 1488 RVA: 0x000192AF File Offset: 0x000174AF
		public CardWidget ToolCardWidget
		{
			get
			{
				return this.toolCard;
			}
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x000192B7 File Offset: 0x000174B7
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

		// Token: 0x060005D2 RID: 1490 RVA: 0x000192F8 File Offset: 0x000174F8
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

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x060005D3 RID: 1491 RVA: 0x00019351 File Offset: 0x00017551
		// (set) Token: 0x060005D4 RID: 1492 RVA: 0x0001935C File Offset: 0x0001755C
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

		// Token: 0x060005D5 RID: 1493 RVA: 0x00019542 File Offset: 0x00017742
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.description.color = new Color(0.94f, 0.78f, 0.32f);
		}

		// Token: 0x060005D6 RID: 1494 RVA: 0x00019563 File Offset: 0x00017763
		public void OnPointerExit(PointerEventData eventData)
		{
			this.description.color = Color.white;
		}

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x060005D7 RID: 1495 RVA: 0x00019578 File Offset: 0x00017778
		// (remove) Token: 0x060005D8 RID: 1496 RVA: 0x000195B0 File Offset: 0x000177B0
		public event Action Click;

		// Token: 0x04000382 RID: 898
		[SerializeField]
		private List<RewardWidget.Pair> bgTable;

		// Token: 0x04000383 RID: 899
		[SerializeField]
		private Button button;

		// Token: 0x04000384 RID: 900
		[SerializeField]
		private ExhibitWidget exhibit;

		// Token: 0x04000385 RID: 901
		[SerializeField]
		private Image bg;

		// Token: 0x04000386 RID: 902
		[SerializeField]
		private TextMeshProUGUI title;

		// Token: 0x04000387 RID: 903
		[SerializeField]
		private TextMeshProUGUI description;

		// Token: 0x04000388 RID: 904
		[SerializeField]
		private CardWidget toolCard;

		// Token: 0x04000389 RID: 905
		private RectTransform _rectTransform;

		// Token: 0x0400038B RID: 907
		private StationReward _stationReward;

		// Token: 0x020001DE RID: 478
		[Serializable]
		public class Pair
		{
			// Token: 0x04000F2F RID: 3887
			public StationRewardType type;

			// Token: 0x04000F30 RID: 3888
			public Sprite sprite;
		}
	}
}
