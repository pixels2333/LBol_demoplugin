using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Black;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x0200032B RID: 811
	[UsedImplicitly]
	public sealed class BailianBlack : Card
	{
		// Token: 0x06000BE7 RID: 3047 RVA: 0x000178CB File Offset: 0x00015ACB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BailianBlackSe>(0, 0, this.IsUpgraded ? 1 : 0, 0, 0.2f);
			yield break;
		}
	}
}
