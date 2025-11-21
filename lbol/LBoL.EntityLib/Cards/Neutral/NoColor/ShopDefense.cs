using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	// Token: 0x020002E3 RID: 739
	[UsedImplicitly]
	public sealed class ShopDefense : Card
	{
		// Token: 0x06000B1E RID: 2846 RVA: 0x00016865 File Offset: 0x00014A65
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
