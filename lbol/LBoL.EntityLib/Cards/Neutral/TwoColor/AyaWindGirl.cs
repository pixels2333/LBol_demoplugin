using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x02000288 RID: 648
	[UsedImplicitly]
	public sealed class AyaWindGirl : Card
	{
		// Token: 0x06000A34 RID: 2612 RVA: 0x000156DD File Offset: 0x000138DD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<WindGirl>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
