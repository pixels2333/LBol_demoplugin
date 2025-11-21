using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	// Token: 0x020002E8 RID: 744
	[UsedImplicitly]
	public sealed class WanderingSoul : Card
	{
		// Token: 0x06000B28 RID: 2856 RVA: 0x0001693E File Offset: 0x00014B3E
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Invincible>(0, base.Value1, 0, 0, 0.2f);
			yield break;
		}
	}
}
