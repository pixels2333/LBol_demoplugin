using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002BE RID: 702
	[UsedImplicitly]
	public sealed class YouxiangWake : Card
	{
		// Token: 0x06000AC3 RID: 2755 RVA: 0x000161D7 File Offset: 0x000143D7
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(false);
			yield return base.BuffAction<YouxiangWakeSe>(base.Value1, 0, base.Value2, this.IsUpgraded ? 1 : 2, 0.2f);
			yield break;
		}
	}
}
