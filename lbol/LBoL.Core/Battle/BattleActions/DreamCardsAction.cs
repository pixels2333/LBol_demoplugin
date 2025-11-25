using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class DreamCardsAction : SimpleAction
	{
		public DreamCardsAction(int count, int playFollowCount = 0)
		{
			this._count = count;
			this._playFollowCount = playFollowCount;
		}
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			if (base.Battle.DrawZone.NotEmpty<Card>())
			{
				this._cards.AddRange(Enumerable.Take<Card>(base.Battle.DrawZone, this._count));
				foreach (Card card in this._cards)
				{
					if (this._playFollowCount > 0 && card.IsFollowCard)
					{
						yield return new PlayCardAction(card);
						this._playFollowCount--;
					}
					else
					{
						MoveCardAction moveCardAction = new MoveCardAction(card, CardZone.Discard)
						{
							DreamCardsAction = this
						};
						yield return moveCardAction;
						card.IsDreamCard = true;
					}
					card = null;
				}
				List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			}
			yield break;
			yield break;
		}
		public IReadOnlyList<Card> Cards
		{
			get
			{
				return this._cards.AsReadOnly();
			}
		}
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
		private readonly int _count;
		private int _playFollowCount;
		private readonly List<Card> _cards = new List<Card>();
	}
}
