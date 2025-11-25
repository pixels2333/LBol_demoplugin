using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.EntityLib.StatusEffects.Marisa;
namespace LBoL.EntityLib.Cards.Character.Marisa
{
	[UsedImplicitly]
	public sealed class PoisonPotion : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.BuffAction<PoisonPotionSe>(base.Value1, 0, 0, 0, 0.2f);
			yield return new AddCardsToDrawZoneAction(Library.CreateCards<Potion>(base.Value2, false), DrawZoneTarget.Random, AddCardsType.Normal);
			yield break;
		}
	}
}
