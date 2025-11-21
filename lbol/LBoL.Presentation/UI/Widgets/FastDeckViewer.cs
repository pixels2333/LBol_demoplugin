using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000057 RID: 87
	public class FastDeckViewer : MonoBehaviour
	{
		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x060004FD RID: 1277 RVA: 0x00015446 File Offset: 0x00013646
		// (set) Token: 0x060004FE RID: 1278 RVA: 0x0001544E File Offset: 0x0001364E
		public int ShowCount { get; set; }

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x060004FF RID: 1279 RVA: 0x00015457 File Offset: 0x00013657
		public int MaxCount
		{
			get
			{
				return 6;
			}
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x0001545A File Offset: 0x0001365A
		private void Awake()
		{
			this.Hide();
		}

		// Token: 0x06000501 RID: 1281 RVA: 0x00015464 File Offset: 0x00013664
		public void Show(IEnumerable<Card> cards)
		{
			List<FastViewCard> list = FastViewCard.CardsToFastViewCards(cards);
			if (list.Count > this.MaxCount)
			{
				list = Enumerable.ToList<FastViewCard>(Enumerable.Take<FastViewCard>(list, this.MaxCount));
				Debug.LogWarning(string.Format("Can't show more than {0} cards in fast deck viewer.", this.MaxCount));
			}
			this.ShowCount = list.Count;
			int showCount = this.ShowCount;
			if (showCount >= 4)
			{
				switch (showCount)
				{
				case 4:
					base.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
					break;
				case 5:
					base.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
					break;
				case 6:
					base.transform.localScale = Vector3.one;
					break;
				}
			}
			else
			{
				base.transform.localScale = new Vector3(1.6f, 1.6f, 1f);
			}
			foreach (ValueTuple<int, CardInFastView> valueTuple in this.widgets.WithIndices<CardInFastView>())
			{
				int item = valueTuple.Item1;
				CardInFastView item2 = valueTuple.Item2;
				if (item < this.ShowCount)
				{
					FastViewCard fastViewCard = list[item];
					item2.SetCard(fastViewCard.Card, fastViewCard.Count, fastViewCard.IsOthers);
				}
				item2.gameObject.SetActive(item < this.ShowCount);
			}
		}

		// Token: 0x06000502 RID: 1282 RVA: 0x000155E0 File Offset: 0x000137E0
		public void Hide()
		{
			foreach (CardInFastView cardInFastView in this.widgets)
			{
				cardInFastView.gameObject.SetActive(false);
			}
		}

		// Token: 0x040002BD RID: 701
		[SerializeField]
		private List<CardInFastView> widgets;
	}
}
