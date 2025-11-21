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
	// Token: 0x020002D2 RID: 722
	[UsedImplicitly]
	public sealed class RinDraw : Card
	{
		// Token: 0x06000AFF RID: 2815 RVA: 0x0001667B File Offset: 0x0001487B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<RinDrawSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
