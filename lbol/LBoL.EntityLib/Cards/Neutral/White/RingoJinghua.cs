using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.White
{
	[UsedImplicitly]
	public sealed class RingoJinghua : Card
	{
		public override Interaction Precondition()
		{
			if (this.IsUpgraded)
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(Enumerable.Concat<Card>(Enumerable.Concat<Card>(base.Battle.HandZone, base.Battle.DrawZoneToShow), base.Battle.DiscardZone), (Card card) => card != this));
				if (!list.Empty<Card>())
				{
					return new SelectCardInteraction(0, base.Value1, list, SelectedCardHandling.DoNothing);
				}
				return null;
			}
			else
			{
				List<Card> list2 = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card card) => card != this));
				if (!list2.Empty<Card>())
				{
					return new SelectHandInteraction(0, base.Value1, list2);
				}
				return null;
			}
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition != null)
			{
				IReadOnlyList<Card> readOnlyList = (this.IsUpgraded ? ((SelectCardInteraction)precondition).SelectedCards : ((SelectHandInteraction)precondition).SelectedCards);
				if (readOnlyList.Count > 0)
				{
					yield return new ExileManyCardAction(readOnlyList);
				}
			}
			yield return new GainPowerAction(base.Value2);
			yield break;
		}
	}
}
