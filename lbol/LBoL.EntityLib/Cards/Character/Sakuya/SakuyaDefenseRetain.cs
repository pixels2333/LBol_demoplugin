using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class SakuyaDefenseRetain : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.DefenseAction(true);
			yield return base.BuffAction<TurnStartDontLoseBlock>(1, 0, 0, 0, 0.2f);
			yield break;
		}
	}
}
