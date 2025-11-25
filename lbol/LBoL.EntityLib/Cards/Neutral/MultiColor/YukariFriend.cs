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
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.StatusEffects.Neutral.MultiColor;
namespace LBoL.EntityLib.Cards.Neutral.MultiColor
{
	[UsedImplicitly]
	public sealed class YukariFriend : Card
	{
		[UsedImplicitly]
		public int Fire
		{
			get
			{
				if (base.Battle == null)
				{
					return 0;
				}
				return Enumerable.Count<Card>(base.Battle.TurnCardUsageHistory, (Card c) => c.CardType == CardType.Defense);
			}
		}
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}
		public override IEnumerable<BattleAction> GetPassiveActions()
		{
			if (!base.Summoned || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			base.Loyalty += base.PassiveCost;
			if (this.Fire > 0)
			{
				int num;
				for (int i = 0; i < base.Battle.FriendPassiveTimes; i = num + 1)
				{
					if (base.Battle.BattleShouldEnd)
					{
						yield break;
					}
					yield return base.BuffAction<Firepower>(this.Fire, 0, 0, 0, 0.2f);
					num = i;
				}
			}
			yield break;
		}
		protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
		{
			if (precondition == null || ((MiniSelectCardInteraction)precondition).SelectedCard.FriendToken == FriendToken.Active)
			{
				base.Loyalty += base.ActiveCost;
				yield return base.SkillAnime;
				if (base.Battle.ExileZone.Count > 0)
				{
					SelectCardInteraction interaction = new SelectCardInteraction(1, 1, base.Battle.ExileZone, SelectedCardHandling.DoNothing)
					{
						Source = this
					};
					yield return new InteractionAction(interaction, false);
					Card card = Enumerable.FirstOrDefault<Card>(interaction.SelectedCards);
					if (card != null)
					{
						yield return new MoveCardAction(card, CardZone.Hand);
					}
					interaction = null;
				}
			}
			else
			{
				base.Loyalty += base.UltimateCost;
				base.UltimateUsed = true;
				if (!base.Battle.BattleShouldEnd)
				{
					yield return base.BuffAction<YukariFriendSe>(1, 0, 0, 0, 0.2f);
				}
			}
			yield break;
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<CardUsingEventArgs>(base.Battle.CardUsed, delegate(CardUsingEventArgs _)
			{
				if (base.Zone == CardZone.Hand)
				{
					this.NotifyChanged();
				}
			}, (GameEventPriority)0);
		}
	}
}
