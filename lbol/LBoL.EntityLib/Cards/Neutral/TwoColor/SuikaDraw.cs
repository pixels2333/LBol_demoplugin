using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002B1 RID: 689
	[UsedImplicitly]
	public sealed class SuikaDraw : Card
	{
		// Token: 0x06000AA2 RID: 2722 RVA: 0x00015F33 File Offset: 0x00014133
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Value1 > base.Battle.HandZone.Count)
			{
				yield return new DrawCardsToSpecificAction(base.Value1);
			}
			yield break;
		}
	}
}
