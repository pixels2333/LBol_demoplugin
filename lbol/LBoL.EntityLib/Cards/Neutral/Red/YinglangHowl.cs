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
	[UsedImplicitly]
	public sealed class YinglangHowl : Card
	{
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
