using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004D1 RID: 1233
	[UsedImplicitly]
	public sealed class SummerIce : Card
	{
		// Token: 0x06001057 RID: 4183 RVA: 0x0001CE47 File Offset: 0x0001B047
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainTurnManaAction(base.Mana);
			yield break;
		}
	}
}
