using System;
using System.Collections.Generic;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Colorless.YijiSkills
{
	// Token: 0x0200030E RID: 782
	public sealed class YijiFirepower : OptionCard
	{
		// Token: 0x06000B98 RID: 2968 RVA: 0x0001739D File Offset: 0x0001559D
		public override IEnumerable<BattleAction> TakeEffectActions()
		{
			yield return base.BuffAction<TempFirepower>(base.Value1, 0, 0, 0, 0.2f);
			Card[] array = base.Battle.RollCards(new CardWeightTable(RarityWeightTable.OnlyCommon, OwnerWeightTable.Valid, CardTypeWeightTable.OnlyAttack, false), base.Value2, null);
			if (array.Length != 0)
			{
				foreach (Card card in array)
				{
					card.IsExile = true;
					card.IsEthereal = true;
					card.IsPurified = true;
				}
				yield return new AddCardsToHandAction(array);
			}
			yield break;
		}
	}
}
