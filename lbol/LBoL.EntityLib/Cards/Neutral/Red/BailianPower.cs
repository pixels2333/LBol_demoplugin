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
	// Token: 0x020002C2 RID: 706
	[UsedImplicitly]
	public sealed class BailianPower : Card
	{
		// Token: 0x06000ACC RID: 2764 RVA: 0x00016297 File Offset: 0x00014497
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Firepower>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
