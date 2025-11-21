using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Dolls;

namespace LBoL.EntityLib.Cards.Character.Alice
{
	// Token: 0x020004E7 RID: 1255
	[UsedImplicitly]
	public sealed class DeployShanghai : Card
	{
		// Token: 0x0600109E RID: 4254 RVA: 0x0001D206 File Offset: 0x0001B406
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddDollAction(Library.CreateDoll<Shanghai>());
			yield break;
		}
	}
}
