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
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Koishi
{
	// Token: 0x02000072 RID: 114
	[UsedImplicitly]
	public sealed class ButterflyDreamSe : StatusEffect
	{
		// Token: 0x06000189 RID: 393 RVA: 0x000050F8 File Offset: 0x000032F8
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x0600018A RID: 394 RVA: 0x0000511C File Offset: 0x0000331C
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Level, (CardConfig config) => config.Rarity == Rarity.Rare && config.Id != "ButterflyDream" && !config.IsXCost);
			foreach (Card card in array)
			{
				card.IsReplenish = true;
				card.IsEthereal = true;
				card.IsExile = true;
				card.SetBaseCost(ManaGroup.Anys(card.ConfigCost.Amount));
			}
			yield return new AddCardsToDrawZoneAction(array, DrawZoneTarget.Top, AddCardsType.Normal);
			yield break;
		}
	}
}
