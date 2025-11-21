using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	// Token: 0x020002A5 RID: 677
	[UsedImplicitly]
	public sealed class RuowujiCard : Card
	{
		// Token: 0x06000A84 RID: 2692 RVA: 0x00015CCA File Offset: 0x00013ECA
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			foreach (BattleAction battleAction in base.DebuffAction<Drowning>(selector.GetUnits(base.Battle), base.Value1, 0, 0, 0, true, 0.2f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
