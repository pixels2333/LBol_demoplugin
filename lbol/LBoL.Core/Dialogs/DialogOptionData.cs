using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
namespace LBoL.Core.Dialogs
{
	public class DialogOptionData
	{
		public bool IsActive { get; set; } = true;
		public string Title { get; private set; }
		public string Content { get; private set; }
		public void AddCard(Card card, bool isRandomResult)
		{
			this._cards.Add(new DialogOptionData.CardEntry
			{
				Card = card,
				IsRandomResult = isRandomResult
			});
		}
		public void AddExhibit(Exhibit exhibit, bool isRandomResult)
		{
			this._exhibits.Add(new DialogOptionData.ExhibitEntry
			{
				Exhibit = exhibit,
				IsRandomResult = isRandomResult
			});
		}
		public void AddTooltip(string title, string content)
		{
			this.Title = title;
			this.Content = content;
		}
		public bool IsEmpty
		{
			get
			{
				return this._cards.Empty<DialogOptionData.CardEntry>() && this._exhibits.Empty<DialogOptionData.ExhibitEntry>();
			}
		}
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
		public IEnumerable<Card> GetCards(bool containsRandomResult)
		{
			if (!containsRandomResult)
			{
				return Enumerable.Select<DialogOptionData.CardEntry, Card>(Enumerable.Where<DialogOptionData.CardEntry>(this._cards, (DialogOptionData.CardEntry c) => !c.IsRandomResult), (DialogOptionData.CardEntry c) => c.Card);
			}
			return Enumerable.Select<DialogOptionData.CardEntry, Card>(this._cards, (DialogOptionData.CardEntry c) => c.Card);
		}
		public IEnumerable<Exhibit> GetExhibits(bool containsRandomResult)
		{
			if (!containsRandomResult)
			{
				return Enumerable.Select<DialogOptionData.ExhibitEntry, Exhibit>(Enumerable.Where<DialogOptionData.ExhibitEntry>(this._exhibits, (DialogOptionData.ExhibitEntry e) => !e.IsRandomResult), (DialogOptionData.ExhibitEntry e) => e.Exhibit);
			}
			return Enumerable.Select<DialogOptionData.ExhibitEntry, Exhibit>(this._exhibits, (DialogOptionData.ExhibitEntry e) => e.Exhibit);
		}
		private readonly List<DialogOptionData.CardEntry> _cards = new List<DialogOptionData.CardEntry>();
		private readonly List<DialogOptionData.ExhibitEntry> _exhibits = new List<DialogOptionData.ExhibitEntry>();
		private struct CardEntry
		{
			public Card Card { readonly get; set; }
			public bool IsRandomResult { readonly get; set; }
		}
		private struct ExhibitEntry
		{
			public Exhibit Exhibit { readonly get; set; }
			public bool IsRandomResult { readonly get; set; }
		}
	}
}
