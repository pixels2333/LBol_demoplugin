using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Cards.Character.Alice
{
	[UsedImplicitly]
	public sealed class DoubleActiveTrigger : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Doll doll = Enumerable.FirstOrDefault<Doll>(base.Battle.Player.Dolls);
			if (doll != null)
			{
				yield return new TriggerDollActiveAction(doll, false);
				yield return new TriggerDollActiveAction(doll, true);
			}
			yield break;
		}
	}
}
