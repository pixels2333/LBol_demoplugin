using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x0200029F RID: 671
	[UsedImplicitly]
	public sealed class PoisonTeaParty : Card
	{
		// Token: 0x06000A75 RID: 2677 RVA: 0x00015BD9 File Offset: 0x00013DD9
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.HealAction(base.Value1);
			yield return base.DebuffAction<Poison>(base.Battle.Player, base.Value1, 0, 0, 0, true, 0.2f);
			yield return base.DebuffAction<Poison>(selector.SelectedEnemy, base.Value2, 0, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
