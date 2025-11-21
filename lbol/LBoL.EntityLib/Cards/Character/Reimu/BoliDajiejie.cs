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
	// Token: 0x020003C6 RID: 966
	[UsedImplicitly]
	public sealed class BoliDajiejie : Card
	{
		// Token: 0x06000DA0 RID: 3488 RVA: 0x00019885 File Offset: 0x00017A85
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BoliDajiejieSe>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
