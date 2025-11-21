using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LBoL.Core.Cards;

namespace LBoL.Presentation.UI.Widgets
{
	// Token: 0x02000058 RID: 88
	public class FastViewCard
	{
		// Token: 0x06000504 RID: 1284 RVA: 0x00015640 File Offset: 0x00013840
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

		// Token: 0x06000505 RID: 1285 RVA: 0x0001571B File Offset: 0x0001391B
		private FastViewCard(Card card, int count)
		{
			this.Card = card;
			this.Count = count;
		}

		// Token: 0x040002BF RID: 703
		public const int MaxCount = 6;

		// Token: 0x040002C0 RID: 704
		public readonly Card Card;

		// Token: 0x040002C1 RID: 705
		public readonly int Count;

		// Token: 0x040002C2 RID: 706
		public bool IsOthers;
	}
}
