using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
namespace LBoL.EntityLib.Cards.Neutral.TwoColor
{
	[UsedImplicitly]
	public sealed class SanaeSummonGod : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Grace>(base.Value2, 0, 0, 0, 0.2f);
			yield return base.BuffAction<SanaeSummonGodSe>(base.Value1, 0, 0, this.IsUpgraded ? 1 : 2, 0.2f);
			yield break;
		}
	}
}
