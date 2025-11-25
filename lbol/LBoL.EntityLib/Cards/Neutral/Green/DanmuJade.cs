using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.Cards.Neutral.NoColor;
namespace LBoL.EntityLib.Cards.Neutral.Green
{
	[UsedImplicitly]
	public sealed class DanmuJade : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return base.AttackAction(selector, null);
			yield return new AddCardsToHandAction(Library.CreateCards<PManaCard>(base.Value1, false), AddCardsType.Normal);
			int amount = base.SynergyAmountComplexMana(consumingMana, base.Mana);
			if (amount > 0)
			{
				yield return base.BuffAction<Firepower>(amount * base.Value2, 0, 0, 0, 0.2f);
				yield return base.BuffAction<Spirit>(amount * base.Value2, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
