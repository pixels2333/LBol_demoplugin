using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002D4 RID: 724
	[UsedImplicitly]
	public sealed class TianziMana : Card
	{
		// Token: 0x06000B04 RID: 2820 RVA: 0x000166D3 File Offset: 0x000148D3
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
