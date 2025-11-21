using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002D1 RID: 721
	[UsedImplicitly]
	public sealed class RemiliaJizao : Card
	{
		// Token: 0x06000AFD RID: 2813 RVA: 0x00016663 File Offset: 0x00014863
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			yield return base.BuffAction<CantDrawThisTurn>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
