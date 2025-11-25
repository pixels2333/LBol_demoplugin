using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.Black;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class EventHorizon : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Burst>(1, 0, 0, 0, 0.2f);
			if (base.Value1 > 0)
			{
				yield return base.BuffAction<TempFirepower>(base.Value1, 0, 0, 0, 0.2f);
			}
			yield return base.BuffAction<NextTurnLoseGame>(0, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
