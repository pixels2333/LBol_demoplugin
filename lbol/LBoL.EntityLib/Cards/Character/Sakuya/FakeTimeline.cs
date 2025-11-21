using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x0200038E RID: 910
	[UsedImplicitly]
	public sealed class FakeTimeline : Card
	{
		// Token: 0x06000CF6 RID: 3318 RVA: 0x00018D10 File Offset: 0x00016F10
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.AttackAction(selector, null);
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			yield return base.BuffAction<TimeAuraSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
