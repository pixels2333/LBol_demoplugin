using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Others;
namespace LBoL.EntityLib.Cards.Enemy
{
	public sealed class LarvaCure : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (base.Battle.Player.HasStatusEffect<Poison>())
			{
				yield return PerformAction.Sfx("JingHua", 0f);
				yield return PerformAction.Effect(base.Battle.Player, "JingHua", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return new RemoveStatusEffectAction(base.Battle.Player.GetStatusEffect<Poison>(), true, 0.1f);
			}
			yield break;
		}
	}
}
