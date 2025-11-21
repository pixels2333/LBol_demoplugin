using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Marisa;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000426 RID: 1062
	[UsedImplicitly]
	public sealed class LearnAstrology : Card
	{
		// Token: 0x06000E90 RID: 3728 RVA: 0x0001AA59 File Offset: 0x00018C59
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<LearnAstrologySe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
