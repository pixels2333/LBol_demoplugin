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
namespace LBoL.EntityLib.Cards.DebugCards
{
	[UsedImplicitly]
	public sealed class DebugAddRandomCards : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] cards = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Value1 * 2, (CardConfig config) => config.Id != base.Id);
			List<Card> list = Enumerable.ToList<Card>(Enumerable.Take<Card>(cards, base.Value1));
			yield return new AddCardsToDrawZoneAction(list, DrawZoneTarget.Random, AddCardsType.Normal);
			List<Card> list2 = Enumerable.ToList<Card>(Enumerable.TakeLast<Card>(cards, base.Value1));
			yield return new AddCardsToDiscardAction(list2, AddCardsType.Normal);
			yield break;
		}
	}
}
