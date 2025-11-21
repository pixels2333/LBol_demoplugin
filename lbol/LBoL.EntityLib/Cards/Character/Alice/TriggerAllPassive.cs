using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Alice
{
	// Token: 0x020004F2 RID: 1266
	[UsedImplicitly]
	public sealed class TriggerAllPassive : Card
	{
		// Token: 0x060010AC RID: 4268 RVA: 0x0001D287 File Offset: 0x0001B487
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new TriggerAllDollsPassiveAction();
			yield break;
		}
	}
}
