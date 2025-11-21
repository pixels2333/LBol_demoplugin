using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003D4 RID: 980
	[UsedImplicitly]
	public sealed class FreeFly : Card
	{
		// Token: 0x06000DC6 RID: 3526 RVA: 0x00019B6D File Offset: 0x00017D6D
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<FreeFlySe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
