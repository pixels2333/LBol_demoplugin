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
	// Token: 0x020004C5 RID: 1221
	[UsedImplicitly]
	public sealed class IceMatrix : Card
	{
		// Token: 0x06001036 RID: 4150 RVA: 0x0001CBCC File Offset: 0x0001ADCC
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<IceMatrixSe>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.BuffAction<FrostArmor>(base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
