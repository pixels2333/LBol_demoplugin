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
	// Token: 0x02000458 RID: 1112
	[UsedImplicitly]
	public sealed class BlackEpiphany : Card
	{
		// Token: 0x06000F10 RID: 3856 RVA: 0x0001B3A4 File Offset: 0x000195A4
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MoodEpiphany>(0, 0, 0, 0, 0.2f);
			yield return base.DebuffAction<Tired>(base.Battle.Player, 0, 0, base.Value1, 0, true, 0.2f);
			yield break;
		}
	}
}
