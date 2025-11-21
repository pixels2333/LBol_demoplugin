using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Green;

namespace LBoL.EntityLib.Cards.Neutral.Green
{
	// Token: 0x020002F8 RID: 760
	[UsedImplicitly]
	public sealed class HuiyeMana : Card
	{
		// Token: 0x06000B53 RID: 2899 RVA: 0x00016CFE File Offset: 0x00014EFE
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<HuiyeManaSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
