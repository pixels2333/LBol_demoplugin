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
	// Token: 0x02000454 RID: 1108
	[UsedImplicitly]
	public sealed class Anatta : Card
	{
		// Token: 0x06000F08 RID: 3848 RVA: 0x0001B34A File Offset: 0x0001954A
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<UpgradePeace>(0, 0, 0, 0, 0.2f);
			yield return base.BuffAction<MoodPeace>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
