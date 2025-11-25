using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class FireUpBagualu : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<Charging>(base.Value1, 0, 0, 0, 0.2f);
			Burst statusEffect = base.Battle.Player.GetStatusEffect<Burst>();
			yield return base.BuffAction<Firepower>((statusEffect != null) ? (statusEffect.DamageRate * base.Value2) : base.Value2, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
