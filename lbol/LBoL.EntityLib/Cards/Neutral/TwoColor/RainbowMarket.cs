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
	// Token: 0x020002A1 RID: 673
	[UsedImplicitly]
	public sealed class RainbowMarket : Card
	{
		// Token: 0x06000A79 RID: 2681 RVA: 0x00015C10 File Offset: 0x00013E10
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<RainbowMarketSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
