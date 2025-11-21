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
	// Token: 0x0200028A RID: 650
	[UsedImplicitly]
	public sealed class BlackPoison : Card
	{
		// Token: 0x06000A38 RID: 2616 RVA: 0x0001570D File Offset: 0x0001390D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BlackPoisonSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
