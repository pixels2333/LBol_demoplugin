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
	// Token: 0x020003BE RID: 958
	[UsedImplicitly]
	public sealed class TimeAuraAttack : Card
	{
		// Token: 0x06000D86 RID: 3462 RVA: 0x00019663 File Offset: 0x00017863
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
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
