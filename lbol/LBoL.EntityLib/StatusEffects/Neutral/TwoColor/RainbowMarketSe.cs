using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	public sealed class RainbowMarketSe : StatusEffect
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardExiled, new EventSequencedReactor<CardEventArgs>(this.OnCardExiled));
		}
		private IEnumerable<BattleAction> OnCardExiled(CardEventArgs args)
		{
			if (args.Cause != ActionCause.AutoExile)
			{
				Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.BattleCard, OwnerWeightTable.Valid, CardTypeWeightTable.CanBeLoot, false), base.Level, null);
				if (array.Length != 0)
				{
					base.NotifyActivating();
					yield return new AddCardsToHandAction(array);
				}
			}
			yield break;
		}
	}
}
