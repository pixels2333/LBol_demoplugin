using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.MultiColor;
namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	[UsedImplicitly]
	public sealed class ZhenmiaowanAbility : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<ZhenmiaowanAbilitySe>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
