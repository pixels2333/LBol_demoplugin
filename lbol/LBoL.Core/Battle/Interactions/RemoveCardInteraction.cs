using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.Interactions
{
	// Token: 0x02000151 RID: 337
	public class RemoveCardInteraction : Interaction
	{
		// Token: 0x170004BA RID: 1210
		// (get) Token: 0x06000D86 RID: 3462 RVA: 0x00025712 File Offset: 0x00023912
		public IReadOnlyList<Card> PendingCards { get; }

		// Token: 0x170004BB RID: 1211
		// (get) Token: 0x06000D87 RID: 3463 RVA: 0x0002571A File Offset: 0x0002391A
		// (set) Token: 0x06000D88 RID: 3464 RVA: 0x00025722 File Offset: 0x00023922
		public Card SelectedCard { get; set; }

		// Token: 0x06000D89 RID: 3465 RVA: 0x0002572B File Offset: 0x0002392B
		public RemoveCardInteraction(IEnumerable<Card> cards)
		{
			this.PendingCards = new List<Card>(cards).AsReadOnly();
		}
	}
}
