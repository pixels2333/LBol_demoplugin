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
	// Token: 0x020004EE RID: 1262
	[UsedImplicitly]
	public sealed class GainDollSlot : Card
	{
		// Token: 0x060010A7 RID: 4263 RVA: 0x0001D257 File Offset: 0x0001B457
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddDollSlotAction(base.Value1);
			yield break;
		}
	}
}
