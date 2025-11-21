using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.ExtraWidgets;
using TMPro;
using UnityEngine;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000050 RID: 80
	public class DeckHolder : MonoBehaviour
	{
		// Token: 0x170000CB RID: 203
		// (get) Token: 0x060004AF RID: 1199 RVA: 0x00013592 File Offset: 0x00011792
		public int CardCount
		{
			get
			{
				return this.cards.Count;
			}
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x000135A0 File Offset: 0x000117A0
		public void Clear()
		{
			this.cardLayout.DestroyChildren();
			this.cards.Clear();
			this.size.text = "Cards.Size".LocalizeFormat(new object[] { this.CardCount });
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x000135EC File Offset: 0x000117EC
		public CardWidget AddCardWidget(Card card, bool needShowing = true)
		{
			CardWidget cardWidget = Object.Instantiate<CardWidget>(this.cardTemplate, this.cardLayout);
			cardWidget.Card = card;
			cardWidget.name = "Card:" + card.Id;
			if (needShowing)
			{
				cardWidget.gameObject.AddComponent<ShowingCard>().SetScale(0.8f, 0.9f);
			}
			this.cards.Add(card, cardWidget);
			this.size.text = "Cards.Size".LocalizeFormat(new object[] { this.CardCount });
			return cardWidget;
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x0001367C File Offset: 0x0001187C
		public void SetOrder(IEnumerable<Card> cardList)
		{
			Card card;
			CardWidget cardWidget;
			foreach (KeyValuePair<Card, CardWidget> keyValuePair in this.cards)
			{
				keyValuePair.Deconstruct(ref card, ref cardWidget);
				cardWidget.transform.SetParent(this.tempParent);
			}
			foreach (Card card2 in cardList)
			{
				foreach (KeyValuePair<Card, CardWidget> keyValuePair in this.cards)
				{
					keyValuePair.Deconstruct(ref card, ref cardWidget);
					Card card3 = card;
					CardWidget cardWidget2 = cardWidget;
					if (card3 == card2)
					{
						cardWidget2.transform.SetParent(this.cardLayout);
					}
				}
			}
			foreach (KeyValuePair<Card, CardWidget> keyValuePair in this.cards)
			{
				keyValuePair.Deconstruct(ref card, ref cardWidget);
				CardWidget cardWidget3 = cardWidget;
				if (cardWidget3.transform.parent == this.tempParent)
				{
					cardWidget3.transform.SetParent(this.cardLayout);
				}
			}
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x000137DC File Offset: 0x000119DC
		public void OnLocaleChanged()
		{
			this.size.text = "Cards.Size".LocalizeFormat(new object[] { this.CardCount });
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x00013807 File Offset: 0x00011A07
		public void SetTitle(string t, string d)
		{
			this.title.text = t;
			this.description.text = d;
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x00013824 File Offset: 0x00011A24
		public void RemoveCardsIfContains(IEnumerable<Card> cards)
		{
			Card card = null;
			CardWidget cardWidget = null;
			foreach (Card card2 in cards)
			{
				foreach (KeyValuePair<Card, CardWidget> keyValuePair in this.cards)
				{
					Card card3;
					CardWidget cardWidget2;
					keyValuePair.Deconstruct(ref card3, ref cardWidget2);
					Card card4 = card3;
					CardWidget cardWidget3 = cardWidget2;
					if (card4 == card2)
					{
						card = card4;
						cardWidget = cardWidget3;
						break;
					}
				}
			}
			if (card != null)
			{
				this.cards.Remove(card);
				Object.Destroy(cardWidget.gameObject);
			}
		}

		// Token: 0x0400027F RID: 639
		[SerializeField]
		private RectTransform cardLayout;

		// Token: 0x04000280 RID: 640
		[SerializeField]
		private RectTransform tempParent;

		// Token: 0x04000281 RID: 641
		[SerializeField]
		private CardWidget cardTemplate;

		// Token: 0x04000282 RID: 642
		[SerializeField]
		private TextMeshProUGUI title;

		// Token: 0x04000283 RID: 643
		[SerializeField]
		private TextMeshProUGUI size;

		// Token: 0x04000284 RID: 644
		[SerializeField]
		private TextMeshProUGUI description;

		// Token: 0x04000285 RID: 645
		public readonly AssociationList<Card, CardWidget> cards = new AssociationList<Card, CardWidget>();

		// Token: 0x04000286 RID: 646
		private const float DefaultScale = 0.8f;

		// Token: 0x04000287 RID: 647
		private const float HoveredScale = 0.9f;
	}
}
