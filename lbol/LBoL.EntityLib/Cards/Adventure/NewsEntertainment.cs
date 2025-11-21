using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004F9 RID: 1273
	[UsedImplicitly]
	public sealed class NewsEntertainment : Card
	{
		// Token: 0x060010BD RID: 4285 RVA: 0x0001D36C File Offset: 0x0001B56C
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			int cardsToFull = base.Battle.CardsToFull;
			if (cardsToFull > 0)
			{
				yield return new DrawManyCardAction(cardsToFull);
			}
			yield break;
		}
	}
}
