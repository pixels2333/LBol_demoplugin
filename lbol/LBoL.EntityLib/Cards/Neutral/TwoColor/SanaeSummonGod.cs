using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002AA RID: 682
	[UsedImplicitly]
	public sealed class SanaeSummonGod : Card
	{
		// Token: 0x06000A90 RID: 2704 RVA: 0x00015D81 File Offset: 0x00013F81
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Grace>(base.Value2, 0, 0, 0, 0.2f);
			yield return base.BuffAction<SanaeSummonGodSe>(base.Value1, 0, 0, this.IsUpgraded ? 1 : 2, 0.2f);
			yield break;
		}
	}
}
