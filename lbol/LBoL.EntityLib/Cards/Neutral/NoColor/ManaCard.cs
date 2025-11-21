using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.NoColor
{
	// Token: 0x020002DD RID: 733
	[UsedImplicitly]
	public abstract class ManaCard : Card
	{
		// Token: 0x06000B12 RID: 2834 RVA: 0x00016779 File Offset: 0x00014979
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
