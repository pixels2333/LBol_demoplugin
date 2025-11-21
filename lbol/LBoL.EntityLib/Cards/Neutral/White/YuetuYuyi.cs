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
	// Token: 0x02000280 RID: 640
	[UsedImplicitly]
	public sealed class YuetuYuyi : Card
	{
		// Token: 0x06000A1D RID: 2589 RVA: 0x000154CB File Offset: 0x000136CB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Invincible>(0, base.Value1, 0, 0, 0.2f);
			yield break;
		}
	}
}
