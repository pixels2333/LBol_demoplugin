using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Character.Marisa
{
	// Token: 0x02000412 RID: 1042
	[UsedImplicitly]
	public sealed class BurstDraw : Card
	{
		// Token: 0x06000E5C RID: 3676 RVA: 0x0001A684 File Offset: 0x00018884
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BurstDrawSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
