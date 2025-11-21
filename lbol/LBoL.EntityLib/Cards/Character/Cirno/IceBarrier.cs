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
	// Token: 0x020004C0 RID: 1216
	[UsedImplicitly]
	public sealed class IceBarrier : Card
	{
		// Token: 0x06001025 RID: 4133 RVA: 0x0001C9B6 File Offset: 0x0001ABB6
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<FrostArmor>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
