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
	// Token: 0x020001BE RID: 446
	[UsedImplicitly]
	public sealed class FixBook : Exhibit
	{
		// Token: 0x0600066D RID: 1645 RVA: 0x0000ECD8 File Offset: 0x0000CED8
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, new EventSequencedReactor<CardUsingEventArgs>(this.OnCardUsed));
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x0000ECF7 File Offset: 0x0000CEF7
		private IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
		{
			if (base.Owner.IsInTurn && args.Card.CardType == CardType.Tool)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}

		// Token: 0x0600066F RID: 1647 RVA: 0x0000ED0E File Offset: 0x0000CF0E
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
