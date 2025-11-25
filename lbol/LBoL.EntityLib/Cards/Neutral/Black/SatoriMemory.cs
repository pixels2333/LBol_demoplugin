using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Cards.Neutral.Black
{
	[UsedImplicitly]
	public sealed class SatoriMemory : Card
	{
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			Card[] array = base.Battle.DiscardZone.SampleManyOrAll(base.Value1, base.GameRun.BattleRng);
			if (array.Length != 0)
			{
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(array, false, false, false)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				Card card = interaction.SelectedCard;
				yield return new MoveCardAction(card, CardZone.Hand);
				if (base.TriggeredAnyhow)
				{
					card.SetTurnCost(base.Mana);
				}
				interaction = null;
				card = null;
			}
			yield break;
		}
	}
}
