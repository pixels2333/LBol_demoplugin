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
	// Token: 0x02000286 RID: 646
	[UsedImplicitly]
	public sealed class AliceAttack : Card
	{
		// Token: 0x06000A30 RID: 2608 RVA: 0x000156A6 File Offset: 0x000138A6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
