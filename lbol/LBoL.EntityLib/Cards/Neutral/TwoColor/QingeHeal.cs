using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002A0 RID: 672
	[UsedImplicitly]
	public sealed class QingeHeal : Card
	{
		// Token: 0x06000A77 RID: 2679 RVA: 0x00015BF8 File Offset: 0x00013DF8
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<TempFirepower>(base.Value1, 0, 0, 0, 0.2f);
			yield return base.HealAction(base.Value2);
			yield return base.DebuffAction<NextTurnLoseHp>(base.Battle.Player, base.Value2, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
