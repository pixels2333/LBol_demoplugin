using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.White;

namespace LBoL.EntityLib.Cards.Neutral.White
{
	// Token: 0x02000281 RID: 641
	[UsedImplicitly]
	public sealed class YukariFlyObject : Card
	{
		// Token: 0x06000A1F RID: 2591 RVA: 0x000154E3 File Offset: 0x000136E3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<YukariFlyObjectSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
