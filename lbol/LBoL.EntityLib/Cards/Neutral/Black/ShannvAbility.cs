using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Black;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x0200033E RID: 830
	[UsedImplicitly]
	public sealed class ShannvAbility : Card
	{
		// Token: 0x06000C14 RID: 3092 RVA: 0x00017BBA File Offset: 0x00015DBA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ShannvAbilitySe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
