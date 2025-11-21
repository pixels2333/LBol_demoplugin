using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000461 RID: 1121
	[UsedImplicitly]
	public sealed class FakeBullet : Card
	{
		// Token: 0x06000F25 RID: 3877 RVA: 0x0001B4D7 File Offset: 0x000196D7
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new FollowAttackAction(selector, base.Value1, true);
			yield break;
		}
	}
}
