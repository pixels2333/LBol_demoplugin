using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public class DrawCardsToSpecificAction : SimpleAction
	{
		public DrawCardsToSpecificAction(int count = 1)
		{
			this._count = count;
		}
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			int num = this._count - base.Battle.HandZone.Count;
			if (num > 0)
			{
				yield return new DrawManyCardAction(num);
			}
			yield break;
		}
		public IReadOnlyList<Card> DrawnCards
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
		private readonly List<Card> _cards = new List<Card>();
	}
}
