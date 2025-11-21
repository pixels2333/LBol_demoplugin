using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x0200031A RID: 794
	[UsedImplicitly]
	public sealed class KokoroAttack : Card
	{
		// Token: 0x06000BBB RID: 3003 RVA: 0x0001760E File Offset: 0x0001580E
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
