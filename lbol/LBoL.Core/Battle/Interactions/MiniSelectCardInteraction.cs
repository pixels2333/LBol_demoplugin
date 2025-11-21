using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.Interactions
{
	// Token: 0x02000150 RID: 336
	public class MiniSelectCardInteraction : Interaction
	{
		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x06000D7F RID: 3455 RVA: 0x000256B2 File Offset: 0x000238B2
		// (set) Token: 0x06000D80 RID: 3456 RVA: 0x000256BA File Offset: 0x000238BA
		public Card SelectedCard { get; set; }

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x06000D81 RID: 3457 RVA: 0x000256C3 File Offset: 0x000238C3
		public IReadOnlyList<Card> PendingCards { get; }

		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x06000D82 RID: 3458 RVA: 0x000256CB File Offset: 0x000238CB
		public bool HasSlideInAnimation { get; }

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x06000D83 RID: 3459 RVA: 0x000256D3 File Offset: 0x000238D3
		public bool IsAddCardToDeck { get; }

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x06000D84 RID: 3460 RVA: 0x000256DB File Offset: 0x000238DB
		public bool CanSkip { get; }

		// Token: 0x06000D85 RID: 3461 RVA: 0x000256E3 File Offset: 0x000238E3
		public MiniSelectCardInteraction(IEnumerable<Card> cards, bool hasSlideInAnimation = false, bool isAddCardToDeck = false, bool canSkip = false)
		{
			this.PendingCards = new List<Card>(cards).AsReadOnly();
			this.HasSlideInAnimation = hasSlideInAnimation;
			this.IsAddCardToDeck = isAddCardToDeck;
			this.CanSkip = canSkip;
		}
	}
}
