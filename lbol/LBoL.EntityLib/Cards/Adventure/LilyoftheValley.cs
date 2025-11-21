using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Adventure
{
	// Token: 0x020004F7 RID: 1271
	[UsedImplicitly]
	public sealed class LilyoftheValley : Card
	{
		// Token: 0x060010B9 RID: 4281 RVA: 0x0001D335 File Offset: 0x0001B535
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
