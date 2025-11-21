using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002A8 RID: 680
	[UsedImplicitly]
	public sealed class SanaeRice : Card
	{
		// Token: 0x06000A8C RID: 2700 RVA: 0x00015D51 File Offset: 0x00013F51
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new RemoveAllNegativeStatusEffectAction(base.Battle.Player, 0.2f);
			yield return base.HealAction(base.Value1);
			yield break;
		}
	}
}
