using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	// Token: 0x020003A5 RID: 933
	[UsedImplicitly]
	public sealed class SakuyaDefenseRetain : Card
	{
		// Token: 0x06000D47 RID: 3399 RVA: 0x000192A2 File Offset: 0x000174A2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<TurnStartDontLoseBlock>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
