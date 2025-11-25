using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Sakuya;
namespace LBoL.EntityLib.Cards.Character.Sakuya
{
	[UsedImplicitly]
	public sealed class GainKnife : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<GainKnifeSe>(base.Value1, 0, 0, 0, 0.2f);
			if (base.Value2 > 0)
			{
				yield return new AddCardsToHandAction(Library.CreateCards<Knife>(base.Value2, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
