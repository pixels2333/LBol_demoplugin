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
	// Token: 0x020002F2 RID: 754
	[UsedImplicitly]
	public sealed class YukariFriend : Card
	{
		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06000B3D RID: 2877 RVA: 0x00016ADE File Offset: 0x00014CDE
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

		// Token: 0x06000B3E RID: 2878 RVA: 0x00016B19 File Offset: 0x00014D19
		public override IEnumerable<BattleAction> OnTurnEndingInHand()
		{
			return this.GetPassiveActions();
		}

		// Token: 0x06000B3F RID: 2879 RVA: 0x00016B21 File Offset: 0x00014D21
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

		// Token: 0x06000B40 RID: 2880 RVA: 0x00016B31 File Offset: 0x00014D31
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

		// Token: 0x06000B41 RID: 2881 RVA: 0x00016B48 File Offset: 0x00014D48
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
