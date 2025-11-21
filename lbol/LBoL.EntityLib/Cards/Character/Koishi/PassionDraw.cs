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
	// Token: 0x0200048A RID: 1162
	[UsedImplicitly]
	public sealed class PassionDraw : Card
	{
		// Token: 0x06000F89 RID: 3977 RVA: 0x0001BBD5 File Offset: 0x00019DD5
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<PassionDrawSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
