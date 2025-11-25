using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	[UsedImplicitly]
	public sealed class QingeDraw : Card
	{
		public override bool DiscardCard
		{
			get
			{
				return true;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			DrawManyCardAction drawAction = new DrawManyCardAction(base.Value1);
			yield return drawAction;
			IEnumerable<Card> drawnCards = drawAction.DrawnCards;
			List<Card> list = new List<Card>();
			foreach (Card card in drawnCards)
			{
				if (card.Zone == CardZone.Hand && (card.Cost.Amount > 1 || card.IsForbidden))
				{
					list.Add(card);
				}
			}
			if (list.Count > 0)
			{
				yield return new DiscardManyAction(list);
			}
			yield break;
		}
	}
}
