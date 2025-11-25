using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Character.Koishi
{
	[UsedImplicitly]
	public sealed class KoishiDrawTop : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			yield return new DrawManyCardAction(base.Value1);
			if (base.Battle.BattleShouldEnd || base.Battle.HandZone.Count == 0)
			{
				yield break;
			}
			SelectHandInteraction interaction = new SelectHandInteraction(base.Value2, base.Value2, base.Battle.HandZone)
			{
				Source = this
			};
			yield return new InteractionAction(interaction, false);
			if (interaction.SelectedCards.Count > 0)
			{
				List<Card> list = Enumerable.ToList<Card>(Enumerable.Where<Card>(interaction.SelectedCards, (Card card) => card.CanUpgradeAndPositive));
				if (Enumerable.Any<Card>(list))
				{
					yield return new UpgradeCardsAction(list);
				}
				foreach (Card card2 in interaction.SelectedCards)
				{
					yield return new MoveCardToDrawZoneAction(card2, DrawZoneTarget.Top);
				}
				IEnumerator<Card> enumerator = null;
			}
			yield break;
			yield break;
		}
	}
}
