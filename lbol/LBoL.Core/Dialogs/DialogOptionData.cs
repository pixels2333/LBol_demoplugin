using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;

namespace LBoL.Core.Dialogs
{
	// Token: 0x02000123 RID: 291
	public class DialogOptionData
	{
		// Token: 0x17000346 RID: 838
		// (get) Token: 0x06000A58 RID: 2648 RVA: 0x0001D0FE File Offset: 0x0001B2FE
		// (set) Token: 0x06000A59 RID: 2649 RVA: 0x0001D106 File Offset: 0x0001B306
		public bool IsActive { get; set; } = true;

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x06000A5A RID: 2650 RVA: 0x0001D10F File Offset: 0x0001B30F
		// (set) Token: 0x06000A5B RID: 2651 RVA: 0x0001D117 File Offset: 0x0001B317
		public string Title { get; private set; }

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x06000A5C RID: 2652 RVA: 0x0001D120 File Offset: 0x0001B320
		// (set) Token: 0x06000A5D RID: 2653 RVA: 0x0001D128 File Offset: 0x0001B328
		public string Content { get; private set; }

		// Token: 0x06000A5E RID: 2654 RVA: 0x0001D134 File Offset: 0x0001B334
		public void AddCard(Card card, bool isRandomResult)
		{
			this._cards.Add(new DialogOptionData.CardEntry
			{
				Card = card,
				IsRandomResult = isRandomResult
			});
		}

		// Token: 0x06000A5F RID: 2655 RVA: 0x0001D168 File Offset: 0x0001B368
		public void AddExhibit(Exhibit exhibit, bool isRandomResult)
		{
			this._exhibits.Add(new DialogOptionData.ExhibitEntry
			{
				Exhibit = exhibit,
				IsRandomResult = isRandomResult
			});
		}

		// Token: 0x06000A60 RID: 2656 RVA: 0x0001D199 File Offset: 0x0001B399
		public void AddTooltip(string title, string content)
		{
			this.Title = title;
			this.Content = content;
		}

		// Token: 0x17000349 RID: 841
		// (get) Token: 0x06000A61 RID: 2657 RVA: 0x0001D1A9 File Offset: 0x0001B3A9
		public bool IsEmpty
		{
			get
			{
				return this._cards.Empty<DialogOptionData.CardEntry>() && this._exhibits.Empty<DialogOptionData.ExhibitEntry>();
			}
		}

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x06000A62 RID: 2658 RVA: 0x0001D1C8 File Offset: 0x0001B3C8
		public bool ContainsRandomResult
		{
			get
			{
				if (!Enumerable.Any<DialogOptionData.CardEntry>(this._cards, (DialogOptionData.CardEntry c) => c.IsRandomResult))
				{
					return Enumerable.Any<DialogOptionData.ExhibitEntry>(this._exhibits, (DialogOptionData.ExhibitEntry c) => c.IsRandomResult);
				}
				return true;
			}
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x0001D230 File Offset: 0x0001B430
		public IEnumerable<Card> GetCards(bool containsRandomResult)
		{
			if (!containsRandomResult)
			{
				return Enumerable.Select<DialogOptionData.CardEntry, Card>(Enumerable.Where<DialogOptionData.CardEntry>(this._cards, (DialogOptionData.CardEntry c) => !c.IsRandomResult), (DialogOptionData.CardEntry c) => c.Card);
			}
			return Enumerable.Select<DialogOptionData.CardEntry, Card>(this._cards, (DialogOptionData.CardEntry c) => c.Card);
		}

		// Token: 0x06000A64 RID: 2660 RVA: 0x0001D2BC File Offset: 0x0001B4BC
		public IEnumerable<Exhibit> GetExhibits(bool containsRandomResult)
		{
			if (!containsRandomResult)
			{
				return Enumerable.Select<DialogOptionData.ExhibitEntry, Exhibit>(Enumerable.Where<DialogOptionData.ExhibitEntry>(this._exhibits, (DialogOptionData.ExhibitEntry e) => !e.IsRandomResult), (DialogOptionData.ExhibitEntry e) => e.Exhibit);
			}
			return Enumerable.Select<DialogOptionData.ExhibitEntry, Exhibit>(this._exhibits, (DialogOptionData.ExhibitEntry e) => e.Exhibit);
		}

		// Token: 0x0400051A RID: 1306
		private readonly List<DialogOptionData.CardEntry> _cards = new List<DialogOptionData.CardEntry>();

		// Token: 0x0400051B RID: 1307
		private readonly List<DialogOptionData.ExhibitEntry> _exhibits = new List<DialogOptionData.ExhibitEntry>();

		// Token: 0x02000274 RID: 628
		private struct CardEntry
		{
			// Token: 0x170005DC RID: 1500
			// (get) Token: 0x06001310 RID: 4880 RVA: 0x00033C12 File Offset: 0x00031E12
			// (set) Token: 0x06001311 RID: 4881 RVA: 0x00033C1A File Offset: 0x00031E1A
			public Card Card { readonly get; set; }

			// Token: 0x170005DD RID: 1501
			// (get) Token: 0x06001312 RID: 4882 RVA: 0x00033C23 File Offset: 0x00031E23
			// (set) Token: 0x06001313 RID: 4883 RVA: 0x00033C2B File Offset: 0x00031E2B
			public bool IsRandomResult { readonly get; set; }
		}

		// Token: 0x02000275 RID: 629
		private struct ExhibitEntry
		{
			// Token: 0x170005DE RID: 1502
			// (get) Token: 0x06001314 RID: 4884 RVA: 0x00033C34 File Offset: 0x00031E34
			// (set) Token: 0x06001315 RID: 4885 RVA: 0x00033C3C File Offset: 0x00031E3C
			public Exhibit Exhibit { readonly get; set; }

			// Token: 0x170005DF RID: 1503
			// (get) Token: 0x06001316 RID: 4886 RVA: 0x00033C45 File Offset: 0x00031E45
			// (set) Token: 0x06001317 RID: 4887 RVA: 0x00033C4D File Offset: 0x00031E4D
			public bool IsRandomResult { readonly get; set; }
		}
	}
}
