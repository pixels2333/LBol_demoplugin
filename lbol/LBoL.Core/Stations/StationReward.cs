using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
namespace LBoL.Core.Stations
{
	public class StationReward
	{
		public StationRewardType Type { get; private set; }
		public int Money { get; private set; }
		public Exhibit Exhibit { get; private set; }
		public List<Card> Cards { get; private set; }
		public static StationReward CreateMoney(int money)
		{
			return new StationReward
			{
				Type = StationRewardType.Money,
				Money = money
			};
		}
		public static StationReward CreateExhibit(Exhibit exhibit)
		{
			return new StationReward
			{
				Type = StationRewardType.Exhibit,
				Exhibit = exhibit
			};
		}
		public static StationReward CreateCards(IEnumerable<Card> cards)
		{
			return new StationReward
			{
				Type = StationRewardType.Card,
				Cards = Enumerable.ToList<Card>(cards)
			};
		}
		public static StationReward CreateCards(params Card[] cards)
		{
			return new StationReward
			{
				Type = StationRewardType.Card,
				Cards = Enumerable.ToList<Card>(cards)
			};
		}
		public static StationReward CreateToolCard(Card card)
		{
			List<Card> list = new List<Card>();
			list.Add(card);
			List<Card> list2 = list;
			return new StationReward
			{
				Type = StationRewardType.Tool,
				Cards = list2
			};
		}
		public static StationReward CreateRemoveCard()
		{
			return new StationReward
			{
				Type = StationRewardType.RemoveCard
			};
		}
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(this.Type).Append(": ");
			switch (this.Type)
			{
			case StationRewardType.Money:
				stringBuilder.Append(this.Money);
				break;
			case StationRewardType.Card:
				stringBuilder.Append(", ".Join(Enumerable.Select<Card, string>(this.Cards, (Card card) => card.Name)));
				break;
			case StationRewardType.Exhibit:
				stringBuilder.Append(this.Exhibit.Name);
				break;
			case StationRewardType.Tool:
				stringBuilder.Append(", ".Join(Enumerable.Select<Card, string>(this.Cards, (Card card) => card.Name)));
				break;
			case StationRewardType.RemoveCard:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return stringBuilder.ToString();
		}
	}
}
