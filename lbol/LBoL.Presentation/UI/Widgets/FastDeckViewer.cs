using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.Presentation.UI.Widgets
{
	public class FastDeckViewer : MonoBehaviour
	{
		public int ShowCount { get; set; }
		public int MaxCount
		{
			get
			{
				return 6;
			}
		}
		private void Awake()
		{
			this.Hide();
		}
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
		public void Hide()
		{
			foreach (CardInFastView cardInFastView in this.widgets)
			{
				cardInFastView.gameObject.SetActive(false);
			}
		}
		[SerializeField]
		private List<CardInFastView> widgets;
	}
}
