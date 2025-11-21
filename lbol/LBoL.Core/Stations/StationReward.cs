using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;

namespace LBoL.Core.Stations
{
	// Token: 0x020000C9 RID: 201
	public class StationReward
	{
		// Token: 0x170002CC RID: 716
		// (get) Token: 0x060008B2 RID: 2226 RVA: 0x000197ED File Offset: 0x000179ED
		// (set) Token: 0x060008B3 RID: 2227 RVA: 0x000197F5 File Offset: 0x000179F5
		public StationRewardType Type { get; private set; }

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x060008B4 RID: 2228 RVA: 0x000197FE File Offset: 0x000179FE
		// (set) Token: 0x060008B5 RID: 2229 RVA: 0x00019806 File Offset: 0x00017A06
		public int Money { get; private set; }

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x060008B6 RID: 2230 RVA: 0x0001980F File Offset: 0x00017A0F
		// (set) Token: 0x060008B7 RID: 2231 RVA: 0x00019817 File Offset: 0x00017A17
		public Exhibit Exhibit { get; private set; }

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x060008B8 RID: 2232 RVA: 0x00019820 File Offset: 0x00017A20
		// (set) Token: 0x060008B9 RID: 2233 RVA: 0x00019828 File Offset: 0x00017A28
		public List<Card> Cards { get; private set; }

		// Token: 0x060008BA RID: 2234 RVA: 0x00019831 File Offset: 0x00017A31
		public static StationReward CreateMoney(int money)
		{
			return new StationReward
			{
				Type = StationRewardType.Money,
				Money = money
			};
		}

		// Token: 0x060008BB RID: 2235 RVA: 0x00019846 File Offset: 0x00017A46
		public static StationReward CreateExhibit(Exhibit exhibit)
		{
			return new StationReward
			{
				Type = StationRewardType.Exhibit,
				Exhibit = exhibit
			};
		}

		// Token: 0x060008BC RID: 2236 RVA: 0x0001985B File Offset: 0x00017A5B
		public static StationReward CreateCards(IEnumerable<Card> cards)
		{
			return new StationReward
			{
				Type = StationRewardType.Card,
				Cards = Enumerable.ToList<Card>(cards)
			};
		}

		// Token: 0x060008BD RID: 2237 RVA: 0x00019875 File Offset: 0x00017A75
		public static StationReward CreateCards(params Card[] cards)
		{
			return new StationReward
			{
				Type = StationRewardType.Card,
				Cards = Enumerable.ToList<Card>(cards)
			};
		}

		// Token: 0x060008BE RID: 2238 RVA: 0x00019890 File Offset: 0x00017A90
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

		// Token: 0x060008BF RID: 2239 RVA: 0x000198BD File Offset: 0x00017ABD
		public static StationReward CreateRemoveCard()
		{
			return new StationReward
			{
				Type = StationRewardType.RemoveCard
			};
		}

		// Token: 0x060008C0 RID: 2240 RVA: 0x000198CC File Offset: 0x00017ACC
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
