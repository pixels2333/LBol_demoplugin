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
	// Token: 0x02000464 RID: 1124
	[UsedImplicitly]
	public sealed class GainInspiration : Card
	{
		// Token: 0x06000F2C RID: 3884 RVA: 0x0001B542 File Offset: 0x00019742
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<GainInspirationSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
