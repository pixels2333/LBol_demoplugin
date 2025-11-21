using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004F5 RID: 1269
	[UsedImplicitly]
	public sealed class EmpCard : Card
	{
		// Token: 0x060010B3 RID: 4275 RVA: 0x0001D2BE File Offset: 0x0001B4BE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield break;
		}
	}
}
