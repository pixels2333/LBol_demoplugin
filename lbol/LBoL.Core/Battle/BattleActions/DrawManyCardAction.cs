using System;
using System.Collections.Generic;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public class DrawManyCardAction : SimpleAction
	{
		public DrawManyCardAction(int count)
		{
			this._count = count;
		}
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			int num;
			for (int i = 0; i < this._count; i = num)
			{
				DrawCardAction draw = new DrawCardAction();
				yield return draw;
				if (draw.Args.Card != null)
				{
					this._cards.Add(draw.Args.Card);
				}
				draw = null;
				num = i + 1;
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
