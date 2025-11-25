using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public class AddCardsToDrawZoneAction : SimpleEventBattleAction<CardsAddingToDrawZoneEventArgs>
	{
		public AddCardsType PresentationType { get; }
		public AddCardsToDrawZoneAction(IEnumerable<Card> cards, DrawZoneTarget target, AddCardsType presentationType = AddCardsType.Normal)
		{
			base.Args = new CardsAddingToDrawZoneEventArgs
			{
				Cards = Enumerable.ToArray<Card>(cards),
				DrawZoneTarget = target
			};
			this.PresentationType = presentationType;
		}
		protected override void PreEventPhase()
		{
			base.Trigger(base.Battle.CardsAddingToDrawZone);
		}
		protected override void MainPhase()
		{
			List<Card> list = new List<Card>();
			foreach (Card card in Enumerable.Reverse<Card>(base.Args.Cards))
			{
				if (base.Battle.AddCardToDrawZone(card, base.Args.DrawZoneTarget) == CancelCause.None)
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
			base.Trigger(base.Battle.CardsAddedToDrawZone);
		}
	}
}
