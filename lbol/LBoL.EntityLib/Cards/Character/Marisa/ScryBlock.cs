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
	// Token: 0x02000443 RID: 1091
	[UsedImplicitly]
	public sealed class ScryBlock : Card
	{
		// Token: 0x06000EE1 RID: 3809 RVA: 0x0001B0AD File Offset: 0x000192AD
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ScryBlockSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
