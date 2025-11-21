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
	// Token: 0x020002BC RID: 700
	[UsedImplicitly]
	public sealed class YonglinUpgrade : Card
	{
		// Token: 0x06000ABD RID: 2749 RVA: 0x0001618E File Offset: 0x0001438E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.HealAction(base.Value2);
			yield return base.BuffAction<YonglinUpgradeSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
