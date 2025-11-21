using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003EC RID: 1004
	[UsedImplicitly]
	public sealed class ReimuDrink : Card
	{
		// Token: 0x06000DFE RID: 3582 RVA: 0x00019F8D File Offset: 0x0001818D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Grace>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
