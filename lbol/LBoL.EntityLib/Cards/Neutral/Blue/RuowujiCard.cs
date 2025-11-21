using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;

namespace LBoL.EntityLib.Cards.Neutral.Blue
{
	// Token: 0x02000320 RID: 800
	[UsedImplicitly]
	public sealed class RuowujiCard : Card
	{
		// Token: 0x06000BCA RID: 3018 RVA: 0x000176C9 File Offset: 0x000158C9
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
