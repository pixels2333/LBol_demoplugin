using System;
using System.Collections.Generic;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.Interactions
{
	// Token: 0x02000156 RID: 342
	public class SelectHandInteraction : Interaction
	{
		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x06000D9C RID: 3484 RVA: 0x000258FE File Offset: 0x00023AFE
		public int Min { get; }

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x06000D9D RID: 3485 RVA: 0x00025906 File Offset: 0x00023B06
		public int Max { get; }

		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x06000D9E RID: 3486 RVA: 0x0002590E File Offset: 0x00023B0E
		// (set) Token: 0x06000D9F RID: 3487 RVA: 0x00025916 File Offset: 0x00023B16
		public bool Sortable { get; set; }

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x06000DA0 RID: 3488 RVA: 0x0002591F File Offset: 0x00023B1F
		public IReadOnlyList<Card> PendingCards { get; }

		// Token: 0x06000DA1 RID: 3489 RVA: 0x00025927 File Offset: 0x00023B27
		public SelectHandInteraction(int min, int max, IEnumerable<Card> cards)
		{
			this.Min = min;
			this.Max = max;
			this.Sortable = this.Max > 1;
			this.PendingCards = new List<Card>(cards).AsReadOnly();
		}

		// Token: 0x170004CB RID: 1227
		// (get) Token: 0x06000DA2 RID: 3490 RVA: 0x0002595D File Offset: 0x00023B5D
		// (set) Token: 0x06000DA3 RID: 3491 RVA: 0x00025968 File Offset: 0x00023B68
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
					throw new InvalidOperationException(string.Format("Invalid {0} count = {1} for {2}", "value", value.Count, "SelectHandInteraction"));
				}
				this._selectedCards = value;
			}
		}

		// Token: 0x0400064B RID: 1611
		private IReadOnlyList<Card> _selectedCards;
	}
}
