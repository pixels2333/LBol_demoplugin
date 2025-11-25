using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Tool
{
	[UsedImplicitly]
	public sealed class ToolWeak : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DebuffAction<Weak>(selector.GetEnemy(base.Battle), 0, base.Value1, 0, 0, true, 0.2f);
			yield break;
		}
	}
}
