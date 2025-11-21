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
	// Token: 0x020004E6 RID: 1254
	[UsedImplicitly]
	public sealed class DeployPenglai : Card
	{
		// Token: 0x0600109B RID: 4251 RVA: 0x0001D1EC File Offset: 0x0001B3EC
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddDollAction(Library.CreateDoll<Penglai>());
			yield break;
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x0001D1F5 File Offset: 0x0001B3F5
		protected override IEnumerable<BattleAction> KickerActions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new AddDollAction(Library.CreateDoll<Penglai>());
			yield return new AddDollAction(Library.CreateDoll<Penglai>());
			yield break;
		}
	}
}
