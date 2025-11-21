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
	// Token: 0x02000372 RID: 882
	[UsedImplicitly]
	public sealed class DebugAddDeckCards : Card
	{
		// Token: 0x06000CA3 RID: 3235 RVA: 0x00018749 File Offset: 0x00016949
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Id != base.Id);
			yield return new AddCardsToDeckAction(array);
			yield break;
		}
	}
}
