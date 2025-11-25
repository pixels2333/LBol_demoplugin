using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Basic;
namespace LBoL.EntityLib.Cards.Tool
{
	[UsedImplicitly]
	public sealed class ToolAmulet : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Amulet>(base.Value1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
