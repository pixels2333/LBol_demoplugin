using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200017F RID: 383
	public class ExileManyCardAction : SimpleAction
	{
		// Token: 0x17000505 RID: 1285
		// (get) Token: 0x06000E7D RID: 3709 RVA: 0x000276C2 File Offset: 0x000258C2
		public Card[] Cards
		{
			get
			{
				return this._cards;
			}
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x000276CA File Offset: 0x000258CA
		public ExileManyCardAction(IEnumerable<Card> cards)
		{
			this._cards = Enumerable.ToArray<Card>(cards);
		}

		// Token: 0x06000E7F RID: 3711 RVA: 0x000276E0 File Offset: 0x000258E0
		protected override void ResolvePhase()
		{
			base.React(new Reactor(Enumerable.Select<Card, ExileCardAction>(this._cards, (Card c) => new ExileCardAction(c, this))), null, default(ActionCause?));
		}

		// Token: 0x0400067B RID: 1659
		private readonly Card[] _cards;
	}
}
