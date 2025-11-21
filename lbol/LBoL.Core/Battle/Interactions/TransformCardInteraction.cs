using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.Interactions
{
	// Token: 0x02000157 RID: 343
	public class TransformCardInteraction : Interaction
	{
		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x06000DA4 RID: 3492 RVA: 0x000259C6 File Offset: 0x00023BC6
		public IReadOnlyList<Card> PendingCards { get; }

		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x06000DA5 RID: 3493 RVA: 0x000259CE File Offset: 0x00023BCE
		// (set) Token: 0x06000DA6 RID: 3494 RVA: 0x000259D6 File Offset: 0x00023BD6
		public Card TransformCard { get; set; }

		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x06000DA7 RID: 3495 RVA: 0x000259DF File Offset: 0x00023BDF
		// (set) Token: 0x06000DA8 RID: 3496 RVA: 0x000259E7 File Offset: 0x00023BE7
		public Card SelectedCard { get; set; }

		// Token: 0x06000DA9 RID: 3497 RVA: 0x000259F0 File Offset: 0x00023BF0
		public TransformCardInteraction(IEnumerable<Card> cards)
		{
			this.PendingCards = new List<Card>(cards).AsReadOnly();
		}
	}
}
