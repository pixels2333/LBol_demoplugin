using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x0200045C RID: 1116
	[UsedImplicitly]
	public sealed class DefaultPassion : Card
	{
		// Token: 0x06000F18 RID: 3864 RVA: 0x0001B404 File Offset: 0x00019604
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.BuffAction<MoodPassion>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
