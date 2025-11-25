using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class ChargingToMana : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Charging statusEffect = base.Battle.Player.GetStatusEffect<Charging>();
			if (statusEffect == null)
			{
				yield break;
			}
			int level = statusEffect.Level;
			yield return new RemoveStatusEffectAction(statusEffect, true, 0.1f);
			yield return new GainManaAction(base.Mana * level);
			yield return new DrawManyCardAction(level);
			yield break;
		}
	}
}
