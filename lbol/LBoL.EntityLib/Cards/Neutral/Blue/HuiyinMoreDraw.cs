using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Blue;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000319 RID: 793
	[UsedImplicitly]
	public sealed class HuiyinMoreDraw : Card
	{
		// Token: 0x06000BB9 RID: 3001 RVA: 0x000175F6 File Offset: 0x000157F6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MoreDraw>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
