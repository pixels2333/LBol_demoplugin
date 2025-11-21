using System;
using System.Collections.Generic;
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
	// Token: 0x02000376 RID: 886
	[UsedImplicitly]
	public sealed class DebugAddRandomCardsToExile : Card
	{
		// Token: 0x06000CAF RID: 3247 RVA: 0x000187F5 File Offset: 0x000169F5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Id != base.Id);
			yield return new AddCardsToExileAction(array);
			yield break;
		}
	}
}
