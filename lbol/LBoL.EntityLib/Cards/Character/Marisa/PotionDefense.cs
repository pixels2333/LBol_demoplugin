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
	// Token: 0x02000438 RID: 1080
	[UsedImplicitly]
	public sealed class PotionDefense : Card
	{
		// Token: 0x06000EC3 RID: 3779 RVA: 0x0001AE78 File Offset: 0x00019078
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<PotionDefenseSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
