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
	// Token: 0x020004CF RID: 1231
	[UsedImplicitly]
	public sealed class ShiverArmor : Card
	{
		// Token: 0x06001052 RID: 4178 RVA: 0x0001CDFE File Offset: 0x0001AFFE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<FrostArmor>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
