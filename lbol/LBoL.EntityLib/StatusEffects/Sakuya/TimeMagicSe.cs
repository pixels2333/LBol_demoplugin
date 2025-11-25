using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	[UsedImplicitly]
	public sealed class TimeMagicSe : StatusEffect
	{
		public ManaGroup Mana { get; set; } = ManaGroup.Anys(3);
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Battle.Player.IsExtraTurn)
			{
				base.NotifyActivating();
				Card[] array = base.Battle.RollCardsWithoutManaLimit(new CardWeightTable(RarityWeightTable.NonCommon, OwnerWeightTable.AllOnes, CardTypeWeightTable.CanBeLoot, false), base.Limit, (CardConfig config) => config.Id != "TimeMagic");
				SelectCardInteraction interaction = new SelectCardInteraction(0, base.Level, array, SelectedCardHandling.DoNothing)
				{
					Source = this
				};
				yield return new InteractionAction(interaction, false);
				IReadOnlyList<Card> selectedCards = interaction.SelectedCards;
				if (selectedCards.Count > 0)
				{
					foreach (Card card in selectedCards)
					{
						card.IsEthereal = true;
						card.IsExile = true;
						card.SetBaseCost(ManaGroup.Anys(card.ConfigCost.Amount));
						card.DecreaseTurnCost(this.Mana);
					}
					yield return new AddCardsToHandAction(selectedCards, AddCardsType.Normal);
				}
				interaction = null;
			}
			yield break;
		}
	}
}
