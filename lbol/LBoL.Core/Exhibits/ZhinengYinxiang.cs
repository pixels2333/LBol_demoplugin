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
	// Token: 0x0200011D RID: 285
	[UsedImplicitly]
	public sealed class ZhinengYinxiang : Exhibit
	{
		// Token: 0x17000340 RID: 832
		// (get) Token: 0x06000A21 RID: 2593 RVA: 0x0001CB1A File Offset: 0x0001AD1A
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

		// Token: 0x06000A22 RID: 2594 RVA: 0x0001CB30 File Offset: 0x0001AD30
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

		// Token: 0x06000A23 RID: 2595 RVA: 0x0001CBA0 File Offset: 0x0001ADA0
		protected override string GetBaseDescription()
		{
			if (base.CardInstanceId == null)
			{
				return base.GetBaseDescription();
			}
			return base.ExtraDescription;
		}

		// Token: 0x06000A24 RID: 2596 RVA: 0x0001CBCA File Offset: 0x0001ADCA
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

		// Token: 0x06000A25 RID: 2597 RVA: 0x0001CBD9 File Offset: 0x0001ADD9
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

		// Token: 0x06000A26 RID: 2598 RVA: 0x0001CBF8 File Offset: 0x0001ADF8
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x0001CC17 File Offset: 0x0001AE17
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

		// Token: 0x06000A28 RID: 2600 RVA: 0x0001CC27 File Offset: 0x0001AE27
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}

		// Token: 0x06000A29 RID: 2601 RVA: 0x0001CC30 File Offset: 0x0001AE30
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
