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
	// Token: 0x020004AD RID: 1197
	[UsedImplicitly]
	public sealed class DoubleMana : Card
	{
		// Token: 0x06000FEA RID: 4074 RVA: 0x0001C469 File Offset: 0x0001A669
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Battle.BattleMana);
			yield return new LockRandomTurnManaAction(base.Value1);
			yield break;
		}
	}
}
