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
	// Token: 0x0200048E RID: 1166
	[UsedImplicitly]
	public sealed class PeaceMemory : Card
	{
		// Token: 0x06000F94 RID: 3988 RVA: 0x0001BCE2 File Offset: 0x00019EE2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MoodPeace>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
