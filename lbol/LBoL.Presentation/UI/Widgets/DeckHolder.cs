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
	public class DeckHolder : MonoBehaviour
	{
		public int CardCount
		{
			get
			{
				return this.cards.Count;
			}
		}
		public void Clear()
		{
			this.cardLayout.DestroyChildren();
			this.cards.Clear();
			this.size.text = "Cards.Size".LocalizeFormat(new object[] { this.CardCount });
		}
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
		public void OnLocaleChanged()
		{
			this.size.text = "Cards.Size".LocalizeFormat(new object[] { this.CardCount });
		}
		public void SetTitle(string t, string d)
		{
			this.title.text = t;
			this.description.text = d;
		}
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
		[SerializeField]
		private RectTransform cardLayout;
		[SerializeField]
		private RectTransform tempParent;
		[SerializeField]
		private CardWidget cardTemplate;
		[SerializeField]
		private TextMeshProUGUI title;
		[SerializeField]
		private TextMeshProUGUI size;
		[SerializeField]
		private TextMeshProUGUI description;
		public readonly AssociationList<Card, CardWidget> cards = new AssociationList<Card, CardWidget>();
		private const float DefaultScale = 0.8f;
		private const float HoveredScale = 0.9f;
	}
}
