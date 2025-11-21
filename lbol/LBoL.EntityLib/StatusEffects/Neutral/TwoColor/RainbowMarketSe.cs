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
	// Token: 0x02000046 RID: 70
	public sealed class RainbowMarketSe : StatusEffect
	{
		// Token: 0x060000D9 RID: 217 RVA: 0x000038AB File Offset: 0x00001AAB
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<CardEventArgs>(base.Battle.CardExiled, new EventSequencedReactor<CardEventArgs>(this.OnCardExiled));
		}

		// Token: 0x060000DA RID: 218 RVA: 0x000038CA File Offset: 0x00001ACA
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
