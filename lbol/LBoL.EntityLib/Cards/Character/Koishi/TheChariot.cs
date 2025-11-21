using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Koishi;

namespace LBoL.EntityLib.Cards.Character.Koishi
{
	// Token: 0x02000493 RID: 1171
	[UsedImplicitly]
	public sealed class TheChariot : Card
	{
		// Token: 0x06000FA9 RID: 4009 RVA: 0x0001BF09 File Offset: 0x0001A109
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new ScryAction(base.Scry);
			yield return base.BuffAction<NextTurnPassion>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
