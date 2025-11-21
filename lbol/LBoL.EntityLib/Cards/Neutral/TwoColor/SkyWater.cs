using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002B0 RID: 688
	[UsedImplicitly]
	public sealed class SkyWater : Card
	{
		// Token: 0x06000AA0 RID: 2720 RVA: 0x00015F1B File Offset: 0x0001411B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			int cardsToFull = base.Battle.CardsToFull;
			if (cardsToFull > 0)
			{
				yield return new DrawManyCardAction(cardsToFull);
			}
			yield break;
		}
	}
}
