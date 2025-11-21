using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000460 RID: 1120
	[UsedImplicitly]
	public sealed class EnterEpiphany : Card
	{
		// Token: 0x06000F23 RID: 3875 RVA: 0x0001B4BF File Offset: 0x000196BF
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MoodEpiphany>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
