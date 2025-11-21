using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core.Battle;

namespace LBoL.Core.Cards
{
	// Token: 0x02000138 RID: 312
	[UsedImplicitly]
	public sealed class WhiteLaser : Card
	{
		// Token: 0x06000BFF RID: 3071 RVA: 0x0002158D File Offset: 0x0001F78D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.DefenseAction(false);
			yield break;
		}
	}
}
