using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002D0 RID: 720
	[UsedImplicitly]
	public sealed class RemiliaBat : Card
	{
		// Token: 0x06000AFB RID: 2811 RVA: 0x0001664B File Offset: 0x0001484B
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TempFirepower>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<Graze>(base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
