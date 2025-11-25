using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Green;
namespace LBoL.EntityLib.Cards.Neutral.Green
{
	[UsedImplicitly]
	public sealed class RangziFanshu : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<RangziFanshuSe>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
