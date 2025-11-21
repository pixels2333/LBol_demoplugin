using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x0200027B RID: 635
	[UsedImplicitly]
	public sealed class SanaeSpirit : Card
	{
		// Token: 0x06000A10 RID: 2576 RVA: 0x000153CE File Offset: 0x000135CE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Spirit>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
