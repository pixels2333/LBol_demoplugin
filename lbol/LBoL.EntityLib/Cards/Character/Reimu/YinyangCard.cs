using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class YinyangCard : YinyangCardBase
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return base.DefenseAction(false);
			yield break;
		}
	}
}
