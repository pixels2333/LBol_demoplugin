using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.MultiColor;

namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	// Token: 0x020002F3 RID: 755
	[UsedImplicitly]
	public sealed class ZhenmiaowanAbility : Card
	{
		// Token: 0x06000B44 RID: 2884 RVA: 0x00016B81 File Offset: 0x00014D81
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ZhenmiaowanAbilitySe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
