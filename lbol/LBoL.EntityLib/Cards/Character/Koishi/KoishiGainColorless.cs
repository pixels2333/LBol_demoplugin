using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000474 RID: 1140
	[UsedImplicitly]
	public sealed class KoishiGainColorless : Card
	{
		// Token: 0x06000F50 RID: 3920 RVA: 0x0001B7C9 File Offset: 0x000199C9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
