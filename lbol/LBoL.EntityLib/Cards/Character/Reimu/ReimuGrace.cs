using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003F0 RID: 1008
	[UsedImplicitly]
	public sealed class ReimuGrace : Card
	{
		// Token: 0x06000E06 RID: 3590 RVA: 0x00019FED File Offset: 0x000181ED
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Grace>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<ReimuGraceSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
