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
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000459 RID: 1113
	[UsedImplicitly]
	public sealed class ButterflyDream : Card
	{
		// Token: 0x06000F12 RID: 3858 RVA: 0x0001B3BC File Offset: 0x000195BC
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Value1, (CardConfig config) => config.Rarity == Rarity.Rare && config.Id != "ButterflyDream" && !config.IsXCost);
			foreach (Card card in array)
			{
				card.IsReplenish = true;
				card.IsEthereal = true;
				card.IsExile = true;
				card.SetBaseCost(ManaGroup.Anys(card.ConfigCost.Amount));
			}
			yield return new AddCardsToDrawZoneAction(array, DrawZoneTarget.Top, AddCardsType.Normal);
			yield return base.BuffAction<ButterflyDreamSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
