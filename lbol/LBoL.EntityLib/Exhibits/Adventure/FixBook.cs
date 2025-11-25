using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.EntityLib.Exhibits.Adventure
{
	[UsedImplicitly]
	public sealed class FixBook : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn && args.Card.CardType == CardType.Tool)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
		protected override IEnumerator SpecialGain(PlayerUnit player)
		{
			this.OnGain(player);
			Card[] shopToolCards = base.GameRun.CurrentStage.GetShopToolCards(base.Value1);
			if (shopToolCards.Length != 0)
			{
				base.GameRun.UpgradeNewDeckCardOnFlags(shopToolCards);
				MiniSelectCardInteraction interaction = new MiniSelectCardInteraction(shopToolCards, true, true, true)
				{
					Source = this
				};
				yield return base.GameRun.InteractionViewer.View(interaction);
				Card selectedCard = interaction.SelectedCard;
				if (selectedCard != null)
				{
					base.GameRun.AddDeckCard(selectedCard, true, new VisualSourceData
					{
						SourceType = VisualSourceType.CardSelect
					});
				}
				interaction = null;
			}
			yield break;
		}
	}
}
