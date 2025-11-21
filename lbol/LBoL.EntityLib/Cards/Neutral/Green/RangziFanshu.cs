using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Green;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x020002FD RID: 765
	[UsedImplicitly]
	public sealed class RangziFanshu : Card
	{
		// Token: 0x06000B65 RID: 2917 RVA: 0x00016ED0 File Offset: 0x000150D0
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<RangziFanshuSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
