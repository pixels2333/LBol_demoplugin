using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Tool
{
	[UsedImplicitly]
	public sealed class ToolMaxHp : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			base.GameRun.GainMaxHp(base.Value1, true, true);
			yield break;
		}
	}
}
