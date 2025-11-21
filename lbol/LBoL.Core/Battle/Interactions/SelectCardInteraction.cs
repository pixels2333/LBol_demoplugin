using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.Interactions
{
	// Token: 0x02000154 RID: 340
	public class SelectCardInteraction : Interaction
	{
		// Token: 0x170004C1 RID: 1217
		// (get) Token: 0x06000D92 RID: 3474 RVA: 0x000257F2 File Offset: 0x000239F2
		public int Min { get; }

		// Token: 0x170004C2 RID: 1218
		// (get) Token: 0x06000D93 RID: 3475 RVA: 0x000257FA File Offset: 0x000239FA
		public int Max { get; }

		// Token: 0x170004C3 RID: 1219
		// (get) Token: 0x06000D94 RID: 3476 RVA: 0x00025802 File Offset: 0x00023A02
		// (set) Token: 0x06000D95 RID: 3477 RVA: 0x0002580A File Offset: 0x00023A0A
		public bool Sortable { get; set; }

		// Token: 0x170004C4 RID: 1220
		// (get) Token: 0x06000D96 RID: 3478 RVA: 0x00025813 File Offset: 0x00023A13
		public IReadOnlyList<Card> PendingCards { get; }

		// Token: 0x170004C5 RID: 1221
		// (get) Token: 0x06000D97 RID: 3479 RVA: 0x0002581B File Offset: 0x00023A1B
		public SelectedCardHandling Handling { get; }

		// Token: 0x06000D98 RID: 3480 RVA: 0x00025823 File Offset: 0x00023A23
		public SelectCardInteraction(int min, int max, IEnumerable<Card> cards, SelectedCardHandling handling = SelectedCardHandling.DoNothing)
		{
			this.Min = min;
			this.Max = max;
			this.Sortable = this.Max > 1;
			this.PendingCards = new List<Card>(cards).AsReadOnly();
			this.Handling = handling;
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x00025861 File Offset: 0x00023A61
		public SelectCardInteraction(int min, int max, bool sortable, IEnumerable<Card> cards, SelectedCardHandling handling = SelectedCardHandling.DoNothing)
		{
			this.Min = min;
			this.Max = max;
			this.Sortable = sortable;
			this.PendingCards = new List<Card>(cards).AsReadOnly();
			this.Handling = handling;
		}

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x06000D9A RID: 3482 RVA: 0x00025898 File Offset: 0x00023A98
		// (set) Token: 0x06000D9B RID: 3483 RVA: 0x000258A0 File Offset: 0x00023AA0
		public IReadOnlyList<Card> SelectedCards
		{
			get
			{
				return this._selectedCards;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (value.Count < this.Min || value.Count > this.Max)
				{
					throw new InvalidOperationException(string.Format("Invalid {0} count = {1} for {2}", "value", value.Count, "SelectCardInteraction"));
				}
				this._selectedCards = value;
			}
		}

		// Token: 0x04000643 RID: 1603
		private IReadOnlyList<Card> _selectedCards;
	}
}
