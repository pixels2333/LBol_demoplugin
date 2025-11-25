using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using UnityEngine;
namespace LBoL.Core.Exhibits
{
	[UsedImplicitly]
	public sealed class ZhinengYinxiang : Exhibit
	{
		[UsedImplicitly]
		public string HoldedCard
		{
			get
			{
				Card card = this.TryGetGameRunCard();
				if (card == null)
				{
					return null;
				}
				return card.Name;
			}
		}
		private Card TryGetGameRunCard()
		{
			GameRunController gameRun = base.GameRun;
			if (gameRun != null)
			{
				int? cardInstanceId = base.CardInstanceId;
				if (cardInstanceId != null)
				{
					int instanceId = cardInstanceId.GetValueOrDefault();
					Card card = Enumerable.FirstOrDefault<Card>(gameRun.BaseDeck, (Card c) => c.InstanceId == instanceId);
					if (card == null)
					{
						Debug.LogError(string.Format("Cannot find card with ID {0} in base-deck", instanceId));
					}
					return card;
				}
			}
			return null;
		}
		protected override string GetBaseDescription()
		{
			if (base.CardInstanceId == null)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}
		protected override IEnumerator SpecialGain(PlayerUnit player)
		{
			IReadOnlyList<Card> baseDeck = base.GameRun.BaseDeck;
			if (baseDeck.Count == 0)
			{
				Debug.LogError("Gain " + this.DebugName + " while base-deck is empty.");
				yield break;
			}
			SelectCardInteraction interaction = new SelectCardInteraction(1, 1, baseDeck, SelectedCardHandling.DoNothing)
			{
				Source = this,
				CanCancel = false
			};
			yield return base.GameRun.InteractionViewer.View(interaction);
			Card card = interaction.SelectedCards[0];
			base.CardInstanceId = new int?(card.InstanceId);
			this.NotifyChanged();
			yield break;
		}
		protected override void OnAdded(PlayerUnit player)
		{
			base.HandleGameRunEvent<CardsEventArgs>(base.GameRun.DeckCardsRemoved, delegate(CardsEventArgs args)
			{
				int? cardInstanceId = base.CardInstanceId;
				if (cardInstanceId != null)
				{
					int instanceId = cardInstanceId.GetValueOrDefault();
					if (Enumerable.Any<Card>(args.Cards, (Card card) => card.InstanceId == instanceId))
					{
						base.CardInstanceId = default(int?);
						this.NotifyChanged();
					}
				}
			});
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			int? cardInstanceId = base.CardInstanceId;
			if (cardInstanceId != null)
			{
				int instanceId = cardInstanceId.GetValueOrDefault();
				Card card2 = Enumerable.FirstOrDefault<Card>(base.Battle.DrawZone, (Card card) => card.InstanceId == instanceId);
				if (card2 == null)
				{
					Debug.LogError(string.Format("Cannot find card with ID {0} in draw-zone", instanceId));
				}
				else
				{
					yield return new MoveCardToDrawZoneAction(card2, DrawZoneTarget.Top);
				}
				base.Blackout = true;
				yield break;
			}
			yield break;
		}
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
		public override IEnumerable<Card> EnumerateRelativeCards()
		{
			foreach (Card card in base.EnumerateRelativeCards())
			{
				yield return card;
			}
			IEnumerator<Card> enumerator = null;
			Card card2 = this.TryGetGameRunCard();
			if (card2 != null)
			{
				yield return card2;
			}
			yield break;
			yield break;
		}
	}
}
