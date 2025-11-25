using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
namespace LBoL.EntityLib.Cards.Neutral.Green
{
	[UsedImplicitly]
	public sealed class LilyChun : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			List<Card> list;
			if (this.IsUpgraded)
			{
				list = Enumerable.ToList<Card>(base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Cost.Amount == 1 && config.Id != base.Id));
			}
			else
			{
				list = Enumerable.ToList<Card>(base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Cost.Amount == 1 && config.Id != base.Id));
			}
			if (list.Count > 0)
			{
				foreach (Card card in list)
				{
					if (this.IsUpgraded)
					{
						card.SetTurnCost(base.Mana);
					}
					card.IsEthereal = true;
					card.IsExile = true;
				}
				yield return new AddCardsToHandAction(list, AddCardsType.Normal);
			}
			yield break;
		}
	}
}
