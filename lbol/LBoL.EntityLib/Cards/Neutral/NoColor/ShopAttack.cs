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
	// Token: 0x020002E2 RID: 738
	[UsedImplicitly]
	public sealed class ShopAttack : Card
	{
		// Token: 0x06000B1C RID: 2844 RVA: 0x00016846 File Offset: 0x00014A46
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return new DrawManyCardAction(base.Value1);
			yield break;
		}
	}
}
