using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200016D RID: 365
	public class DiscardManyAction : SimpleAction
	{
		// Token: 0x06000E20 RID: 3616 RVA: 0x00026F3C File Offset: 0x0002513C
		public DiscardManyAction(IEnumerable<Card> cards)
		{
			this._cards = Enumerable.ToArray<Card>(cards);
		}

		// Token: 0x06000E21 RID: 3617 RVA: 0x00026F50 File Offset: 0x00025150
		protected override void ResolvePhase()
		{
			base.React(new Reactor(Enumerable.Select<Card, DiscardAction>(this._cards, (Card c) => new DiscardAction(c))), null, default(ActionCause?));
		}

		// Token: 0x170004E8 RID: 1256
		// (get) Token: 0x06000E22 RID: 3618 RVA: 0x00026F9C File Offset: 0x0002519C
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04000665 RID: 1637
		private readonly Card[] _cards;
	}
}
