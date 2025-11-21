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
	// Token: 0x020003BF RID: 959
	[UsedImplicitly]
	public sealed class TimeLord : Card
	{
		// Token: 0x06000D88 RID: 3464 RVA: 0x00019682 File Offset: 0x00017882
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TimeAuraSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
