using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002C6 RID: 710
	[UsedImplicitly]
	public sealed class HekaHellRain : Card
	{
		// Token: 0x06000AD8 RID: 2776 RVA: 0x00016334 File Offset: 0x00014534
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<HekaHellRainSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
