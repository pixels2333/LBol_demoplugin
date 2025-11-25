using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public class AddCardsToExileAction : SimpleEventBattleAction<CardsEventArgs>
	{
		public AddCardsType PresentationType { get; }
		public AddCardsToExileAction(params Card[] cards)
			: this(cards, AddCardsType.Normal)
		{
		}
		public AddCardsToExileAction(IEnumerable<Card> cards, AddCardsType presentationType = AddCardsType.Normal)
		{
			base.Args = new CardsEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards)
			};
			this.PresentationType = presentationType;
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.CardsAddingToExile);
		}
		protected override void MainPhase()
		{
			List<Card> list = new List<Card>();
			foreach (Card card in base.Args.Cards)
			{
				if (base.Battle.AddCardToExile(card) == CancelCause.None)
				{
					list.Add(card);
				}
			}
			if (!Enumerable.SequenceEqual<Card>(list, base.Args.Cards))
			{
				base.Args.Cards = list.ToArray();
				base.Args.IsModified = true;
			}
		}
		protected override void PostEventPhase()
		{
			base.Trigger(base.Battle.CardsAddedToExile);
		}
	}
}
