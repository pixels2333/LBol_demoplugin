using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002A9 RID: 681
	[UsedImplicitly]
	public sealed class SanaeStar : Card
	{
		// Token: 0x06000A8E RID: 2702 RVA: 0x00015D69 File Offset: 0x00013F69
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			yield return base.BuffAction<Grace>(base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
