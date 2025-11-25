using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	[UsedImplicitly]
	public sealed class MoonPurify : Card
	{
		public override Interaction Precondition()
		{
			if (!this.IsUpgraded)
			{
				return null;
			}
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card hand) => hand != this && !hand.IsPurified));
			if (list.Count <= 0)
			{
				return null;
			}
			return new SelectHandInteraction(1, 1, list);
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (this.IsUpgraded)
			{
				SelectHandInteraction selectHandInteraction = (SelectHandInteraction)precondition;
				Card card4 = ((selectHandInteraction != null) ? selectHandInteraction.SelectedCards[0] : null);
				if (card4 != null)
				{
					card4.NotifyActivating();
					card4.IsPurified = true;
				}
			}
			else
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => !card.IsPurified && card.Cost.HasTrivialOrHybrid));
				if (list.Count > 0)
				{
					Card card2 = list.Sample(base.GameRun.BattleRng);
					card2.NotifyActivating();
					card2.IsPurified = true;
				}
				else
				{
					List<Card> list2 = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => !card.IsPurified));
					if (list2.Count > 0)
					{
						Card card3 = list2.Sample(base.GameRun.BattleRng);
						card3.NotifyActivating();
						card3.IsPurified = true;
					}
				}
			}
			yield break;
		}
	}
}
