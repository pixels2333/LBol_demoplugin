using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Neutral.Black;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class BailianBlack : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<BailianBlackSe>(0, 0, this.IsUpgraded ? 1 : 0, 0, 0.2f);
			yield break;
		}
	}
}
