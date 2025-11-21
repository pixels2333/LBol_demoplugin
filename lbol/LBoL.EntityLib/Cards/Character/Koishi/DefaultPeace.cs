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
	// Token: 0x0200045D RID: 1117
	[UsedImplicitly]
	public sealed class DefaultPeace : Card
	{
		// Token: 0x06000F1A RID: 3866 RVA: 0x0001B423 File Offset: 0x00019623
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<MoodPeace>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
