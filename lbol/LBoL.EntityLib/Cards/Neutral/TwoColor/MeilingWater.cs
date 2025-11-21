using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200029C RID: 668
	[UsedImplicitly]
	public sealed class MeilingWater : Card
	{
		// Token: 0x06000A6A RID: 2666 RVA: 0x00015AF3 File Offset: 0x00013CF3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Battle.DrawZone.Count > 0)
			{
				int i = 1;
				while (i <= base.Value2 && base.Battle.DrawZone.Count > 0 && !base.Battle.HandIsFull)
				{
					List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.DrawZone, (Card card) => card.CardType == CardType.Attack));
					if (list.Count > 0)
					{
						Card card2 = list.Sample(base.GameRun.BattleRng);
						card2.SetTurnCost(base.Mana);
						yield return new MoveCardAction(card2, CardZone.Hand);
					}
					int num = i;
					i = num + 1;
				}
			}
			yield return base.DebuffAction<MeilingWaterSe>(base.Battle.Player, base.Value1, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
