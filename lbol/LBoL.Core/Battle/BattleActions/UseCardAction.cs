using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Core.Exhibits;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001B2 RID: 434
	public sealed class UseCardAction : BattleAction
	{
		// Token: 0x17000531 RID: 1329
		// (get) Token: 0x06000F69 RID: 3945 RVA: 0x00029700 File Offset: 0x00027900
		public CardUsingEventArgs Args { get; }

		// Token: 0x06000F6A RID: 3946 RVA: 0x00029708 File Offset: 0x00027908
		internal UseCardAction(Card card, UnitSelector selector, ManaGroup consumingMana, bool kicker)
		{
			this.Args = new CardUsingEventArgs
			{
				Card = card,
				Selector = selector,
				ConsumingMana = consumingMana,
				Kicker = kicker
			};
		}

		// Token: 0x06000F6B RID: 3947 RVA: 0x00029738 File Offset: 0x00027938
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.Args.Cause = cause;
			return this;
		}

		// Token: 0x06000F6C RID: 3948 RVA: 0x0002974F File Offset: 0x0002794F
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			this.Args.ActionSource = base.Source;
			return this;
		}

		// Token: 0x17000532 RID: 1330
		// (get) Token: 0x06000F6D RID: 3949 RVA: 0x0002976B File Offset: 0x0002796B
		public override bool IsModified
		{
			get
			{
				return this.Args.IsModified;
			}
		}

		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x06000F6E RID: 3950 RVA: 0x00029778 File Offset: 0x00027978
		public override string[] Modifiers
		{
			get
			{
				return this.Args.Modifiers;
			}
		}

		// Token: 0x17000534 RID: 1332
		// (get) Token: 0x06000F6F RID: 3951 RVA: 0x00029785 File Offset: 0x00027985
		public override CancelCause CancelCause
		{
			get
			{
				return this.Args.CancelCause;
			}
		}

		// Token: 0x17000535 RID: 1333
		// (get) Token: 0x06000F70 RID: 3952 RVA: 0x00029792 File Offset: 0x00027992
		public override bool IsCanceled
		{
			get
			{
				return this.Args.IsCanceled;
			}
		}

		// Token: 0x06000F71 RID: 3953 RVA: 0x0002979F File Offset: 0x0002799F
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}

		// Token: 0x06000F72 RID: 3954 RVA: 0x000297AC File Offset: 0x000279AC
		internal override IEnumerable<Phase> GetPhases()
		{
			base.Battle.PlayedCardInManaFreezeLevel = base.Battle.ManaFreezeLevel;
			yield return base.CreateEventPhase<CardUsingEventArgs>("CardUsing", this.Args, base.Battle.CardUsing);
			if (this.Args.PlayTwice)
			{
				this._twiceTokenCard = this.Args.Card.CloneTwiceToken();
				this._twiceTokenCard.IsPlayTwiceToken = true;
				this._twiceTokenCard.PlayTwiceSourceCard = this.Args.Card;
			}
			yield return base.CreatePhase("Precondition", delegate
			{
				this._precondition = this.Args.Card.Precondition();
				if (this._precondition != null)
				{
					this._precondition.Source = this.Args.Card;
					this.React(new InteractionAction(this._precondition, true), null, default(ActionCause?));
				}
			}, false);
			yield return base.CreatePhase("CardUse/Canceled", delegate
			{
				Interaction precondition = this._precondition;
				if (precondition != null && precondition.IsCanceled)
				{
					this.Battle.CardUsingCanceled.Execute(this.Args);
					this.Args.ForceCancelBecause(CancelCause.UserCanceled);
					return;
				}
				this.Args.CanCancel = false;
				Card card = this.Args.Card;
				card.PlayInTriggered = card.Triggered;
				this.Battle.MoveCardToPlayArea(card);
				int? moneyCost = card.Config.MoneyCost;
				if (moneyCost != null)
				{
					int valueOrDefault = moneyCost.GetValueOrDefault();
					this.Battle.React(new ConsumeMoneyAction(valueOrDefault), card, ActionCause.CardUse);
				}
				this.Battle.React(new ConsumeManaAction(this.Args.ConsumingMana), card, ActionCause.CardUse);
			}, true);
			List<DamageAction> damageActions = new List<DamageAction>();
			yield return base.CreatePhase("CardAction", delegate
			{
				Interaction precondition2 = this._precondition;
				if (precondition2 != null && precondition2.IsCanceled)
				{
					return;
				}
				Card card2 = this.Args.Card;
				if (card2.CardType == CardType.Friend)
				{
					card2.Summoning = !card2.Summoned;
				}
				this.Battle.React(new Reactor(card2.GetActions(this.Args.Selector, this.Args.ConsumingMana, this._precondition, this.Args.Kicker, card2.Summoning, damageActions)), card2, ActionCause.Card);
			}, false);
			if (damageActions.Count > 0)
			{
				yield return base.CreatePhase("Statistics", delegate
				{
					this.Battle.React(new StatisticalTotalDamageAction(damageActions), this.Args.Card, ActionCause.Card);
				}, false);
			}
			yield return base.CreatePhase("AfterUse", delegate
			{
				IEnumerable<BattleAction> enumerable = this.Args.Card.AfterUseAction();
				if (enumerable != null)
				{
					this.Battle.React(new Reactor(enumerable), this.Args.Card, ActionCause.CardUse);
				}
				if (this.Battle.PlayedCardInManaFreezeLevel > 0)
				{
					List<ManaFreezer> list = new List<ManaFreezer>();
					foreach (Card card3 in this.Battle.HandZone)
					{
						ManaFreezer manaFreezer = card3 as ManaFreezer;
						if (manaFreezer != null)
						{
							list.Add(manaFreezer);
						}
					}
					if (list.Count > 0)
					{
						int i = this.Battle.PlayedCardInManaFreezeLevel;
						while (i > 0)
						{
							int l = i;
							ManaFreezer manaFreezer2 = Enumerable.FirstOrDefault<ManaFreezer>(list, (ManaFreezer card) => card.FreezeLevel == l);
							if (manaFreezer2 != null)
							{
								manaFreezer2.FreezeTimes--;
								if (manaFreezer2.FreezeTimes <= 0)
								{
									this.Battle.React(new ExileCardAction(manaFreezer2), null, ActionCause.CardUse);
									break;
								}
								manaFreezer2.NotifyActivating();
								break;
							}
							else
							{
								i--;
							}
						}
					}
					this.Battle.CheckManaFreeze();
					this.Battle.PlayedCardInManaFreezeLevel = 0;
				}
				if (this.Battle.DrawAfterDiscard > 0)
				{
					int drawAfterDiscard = this.Battle.DrawAfterDiscard;
					this.Battle.DrawAfterDiscard = 0;
					this.Battle.React(new DrawManyCardAction(drawAfterDiscard), null, ActionCause.CardUse);
				}
				if (this.Battle.Player.IsInTurn && this.Battle.GameRun.YichuiPiaoFlag > 0 && this.Battle.HandZone.Count == 0)
				{
					YichuiPiao exhibit = this.Battle.Player.GetExhibit<YichuiPiao>();
					exhibit.NotifyActivating();
					this.Battle.React(new DrawCardsToSpecificAction(1), exhibit, ActionCause.Exhibit);
				}
				if (this.Battle.GameRun.IsAutoSeed && this.Battle.GameRun.JadeBoxes.Empty<JadeBox>())
				{
					if (Enumerable.Count<Card>(this.Battle.HandZoneAndPlayArea, (Card card) => card.Summoned) >= 9)
					{
						this.Battle.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.Friend);
					}
				}
			}, false);
			yield return base.CreatePhase("Record", delegate
			{
				this.Battle.RecordCardUsage(this.Args.Card);
			}, false);
			if (this.Args.PlayTwice && this._twiceTokenCard != null && !base.Battle.BattleShouldEnd)
			{
				yield return base.CreatePhase("PlayTwice", delegate
				{
					this.React(new PlayTwiceAction(this._twiceTokenCard, this.Args), null, default(ActionCause?));
				}, false);
			}
			yield return base.CreateEventPhase<CardUsingEventArgs>("CardUsed", this.Args, base.Battle.CardUsed);
			yield break;
		}

		// Token: 0x06000F73 RID: 3955 RVA: 0x000297BC File Offset: 0x000279BC
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}

		// Token: 0x040006A8 RID: 1704
		private Interaction _precondition;

		// Token: 0x040006A9 RID: 1705
		private Card _twiceTokenCard;
	}
}
