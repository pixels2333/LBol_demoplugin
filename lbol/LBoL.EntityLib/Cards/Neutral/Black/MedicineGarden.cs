using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Others;

namespace LBoL.EntityLib.Cards.Neutral.Black
{
	// Token: 0x02000333 RID: 819
	[UsedImplicitly]
	public sealed class MedicineGarden : Card
	{
		// Token: 0x06000BF9 RID: 3065 RVA: 0x000179FB File Offset: 0x00015BFB
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			foreach (Unit unit in selector.GetUnits(base.Battle))
			{
				yield return new HealAction(base.Battle.Player, unit, base.Value1, HealType.Normal, 0.2f);
			}
			Unit[] array = null;
			foreach (BattleAction battleAction in base.DebuffAction<Poison>(selector.GetUnits(base.Battle), base.Value1, 0, 0, 0, true, 0.2f))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}
	}
}
