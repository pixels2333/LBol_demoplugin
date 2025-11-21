using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Cirno;

namespace LBoL.EntityLib.Cards.Character.Cirno
{
	// Token: 0x020004B8 RID: 1208
	[UsedImplicitly]
	public sealed class FriendDraw : Card
	{
		// Token: 0x06001006 RID: 4102 RVA: 0x0001C6E2 File Offset: 0x0001A8E2
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<FriendDrawSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
