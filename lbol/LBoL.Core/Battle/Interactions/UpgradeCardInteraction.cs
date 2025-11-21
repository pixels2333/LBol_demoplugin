using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.Interactions
{
	// Token: 0x02000158 RID: 344
	public class UpgradeCardInteraction : Interaction
	{
		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x06000DAA RID: 3498 RVA: 0x00025A09 File Offset: 0x00023C09
		public IReadOnlyList<Card> PendingCards { get; }

		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x06000DAB RID: 3499 RVA: 0x00025A11 File Offset: 0x00023C11
		// (set) Token: 0x06000DAC RID: 3500 RVA: 0x00025A19 File Offset: 0x00023C19
		public Card SelectedCard { get; set; }

		// Token: 0x06000DAD RID: 3501 RVA: 0x00025A22 File Offset: 0x00023C22
		public UpgradeCardInteraction(IEnumerable<Card> cards)
		{
			this.PendingCards = new List<Card>(cards).AsReadOnly();
		}
	}
}
