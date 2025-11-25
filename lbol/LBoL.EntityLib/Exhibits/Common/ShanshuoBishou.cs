using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class ShanshuoBishou : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				IReadOnlyList<Card> handZone = base.Battle.HandZone;
				int count = handZone.Count;
				SelectHandInteraction interaction = new SelectHandInteraction(0, count, handZone)
				{
					Source = this,
					CanCancel = false
				};
				yield return new InteractionAction(interaction, false);
				IReadOnlyList<Card> cards = interaction.SelectedCards;
				if (cards.Count > 0)
				{
					yield return new DiscardManyAction(cards);
					if (base.Battle.DrawAfterDiscard > 0)
					{
						int drawAfterDiscard = base.Battle.DrawAfterDiscard;
						base.Battle.DrawAfterDiscard = 0;
						yield return new DrawManyCardAction(drawAfterDiscard);
					}
					yield return new DrawManyCardAction(cards.Count);
				}
				base.Blackout = true;
				interaction = null;
				cards = null;
			}
			yield break;
		}
	}
}
