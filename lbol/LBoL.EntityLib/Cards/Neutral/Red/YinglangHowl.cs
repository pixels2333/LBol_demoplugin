using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;

namespace LBoL.EntityLib.Cards.Neutral.Red
{
	// Token: 0x020002D5 RID: 725
	[UsedImplicitly]
	public sealed class YinglangHowl : Card
	{
		// Token: 0x06000B06 RID: 2822 RVA: 0x000166EB File Offset: 0x000148EB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (BattleAction battleAction in base.DebuffAction<TempFirepowerNegative>(selector.GetUnits(base.Battle), base.Value1, 0, 0, 0, true, 0.2f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			foreach (BattleAction battleAction2 in base.DebuffAction<Vulnerable>(selector.GetUnits(base.Battle), 0, base.Value2, 0, 0, true, 0.2f))
			{
				yield return battleAction2;
			}
			enumerator = null;
			yield break;
			yield break;
		}
	}
}
