using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004CB RID: 1227
	[UsedImplicitly]
	public sealed class MoreFriends : Card
	{
		// Token: 0x06001049 RID: 4169 RVA: 0x0001CD9D File Offset: 0x0001AF9D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MoreFriendsSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
