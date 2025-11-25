using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
namespace LBoL.EntityLib.Cards.Character.Reimu
{
	[UsedImplicitly]
	public sealed class ZhujiThreeGod : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			DrawManyCardAction drawAction = new DrawManyCardAction(base.Value1);
			yield return drawAction;
			IReadOnlyList<Card> drawnCards = drawAction.DrawnCards;
			int num = Enumerable.Count<Card>(drawnCards, (Card card) => card.CardType == CardType.Attack);
			int defense = Enumerable.Count<Card>(drawnCards, (Card card) => card.CardType == CardType.Defense);
			if (num > 0)
			{
				yield return base.BuffAction<TempFirepower>(base.Value2 * num, 0, 0, 0, 0.2f);
			}
			if (defense > 0)
			{
				yield return base.BuffAction<TempSpirit>(base.Value2 * defense, 0, 0, 0, 0.2f);
			}
			yield break;
		}
	}
}
