using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Neutral.MultiColor;

namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	// Token: 0x020002F0 RID: 752
	[UsedImplicitly]
	public sealed class PrismriverAll : Card
	{
		// Token: 0x06000B37 RID: 2871 RVA: 0x00016A56 File Offset: 0x00014C56
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			if (base.Value1 > 0)
			{
				yield return new DrawManyCardAction(base.Value1);
			}
			yield return base.BuffAction<PrismriverSe>(0, 0, 0, 0, 0.2f);
			List<Card> list = new List<Card>();
			list.Add(Library.CreateCard<Riguang>());
			list.Add(Library.CreateCard<Yueguang>());
			list.Add(Library.CreateCard<Xingguang>());
			List<Card> list2 = list;
			yield return new AddCardsToDiscardAction(list2, AddCardsType.Normal);
			yield break;
		}
	}
}
