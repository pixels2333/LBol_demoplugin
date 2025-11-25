using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Core.Cards;
namespace LBoL.Presentation.UI.Widgets
{
	public class FastViewCard
	{
		public static List<FastViewCard> CardsToFastViewCards(IEnumerable<Card> cards)
		{
			var list = Enumerable.ToList(Enumerable.OrderByDescending(Enumerable.Select(Enumerable.GroupBy<Card, ValueTuple<string, bool, int?>>(cards, (Card c) => new ValueTuple<string, bool, int?>(c.Id, c.IsUpgraded, c.UpgradeCounter)), ([TupleElementNames(new string[] { "Id", "IsUpgraded", "UpgradeCounter" })] IGrouping<ValueTuple<string, bool, int?>, Card> g) => new
			{
				Card = Enumerable.First<Card>(g),
				Count = Enumerable.Count<Card>(g)
			}), x => x.Count));
			bool flag = list.Count > 6;
			if (list.Count > 6)
			{
				list = Enumerable.ToList(Enumerable.Take(list, 6));
			}
			List<FastViewCard> list2 = Enumerable.ToList<FastViewCard>(Enumerable.Select(list, d => new FastViewCard(d.Card, d.Count)));
			if (flag)
			{
				Enumerable.Last<FastViewCard>(list2).IsOthers = true;
			}
			return list2;
		}
		private FastViewCard(Card card, int count)
		{
			this.Card = card;
			this.Count = count;
		}
		public const int MaxCount = 6;
		public readonly Card Card;
		public readonly int Count;
		public bool IsOthers;
	}
}
