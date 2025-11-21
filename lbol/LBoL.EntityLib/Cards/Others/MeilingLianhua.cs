using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Others
{
	// Token: 0x0200026F RID: 623
	[UsedImplicitly]
	public sealed class MeilingLianhua : Card
	{
		// Token: 0x060009EC RID: 2540 RVA: 0x000150C6 File Offset: 0x000132C6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
