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
	// Token: 0x020004CA RID: 1226
	[UsedImplicitly]
	public sealed class MinengZhuru : Card
	{
		// Token: 0x06001047 RID: 4167 RVA: 0x0001CD85 File Offset: 0x0001AF85
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<MinengZhuruSe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
