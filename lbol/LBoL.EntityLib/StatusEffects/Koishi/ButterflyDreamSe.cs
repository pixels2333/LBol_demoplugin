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
	[UsedImplicitly]
	public sealed class ButterflyDreamSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}
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
