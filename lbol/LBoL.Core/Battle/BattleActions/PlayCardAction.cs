using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000197 RID: 407
	public sealed class PlayCardAction : BattleAction
	{
		// Token: 0x1700051D RID: 1309
		// (get) Token: 0x06000EF3 RID: 3827 RVA: 0x00028565 File Offset: 0x00026765
		public CardUsingEventArgs Args { get; }

		// Token: 0x1700051E RID: 1310
		// (get) Token: 0x06000EF4 RID: 3828 RVA: 0x0002856D File Offset: 0x0002676D
		public CardZone SourceZone { get; }

		// Token: 0x06000EF5 RID: 3829 RVA: 0x00028575 File Offset: 0x00026775
		public PlayCardAction(Card card)
		{
			this.Args = new CardUsingEventArgs
			{
				Card = card,
				Selector = UnitSelector.Nobody
			};
			this.SourceZone = card.Zone;
		}

		// Token: 0x06000EF6 RID: 3830 RVA: 0x000285A6 File Offset: 0x000267A6
		public PlayCardAction(Card card, UnitSelector selector)
		{
			this.Args = new CardUsingEventArgs
			{
				Card = card,
				Selector = selector
			};
			this.SourceZone = card.Zone;
		}

		// Token: 0x06000EF7 RID: 3831 RVA: 0x000285D3 File Offset: 0x000267D3
		public PlayCardAction(Card card, UnitSelector selector, ManaGroup consumingMana)
		{
			this.Args = new CardUsingEventArgs
			{
				Card = card,
				Selector = selector,
				ConsumingMana = consumingMana
			};
			this.SourceZone = card.Zone;
		}

		// Token: 0x06000EF8 RID: 3832 RVA: 0x00028607 File Offset: 0x00026807
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.Args.Cause = cause;
			return this;
		}

		// Token: 0x06000EF9 RID: 3833 RVA: 0x0002861E File Offset: 0x0002681E
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			this.Args.ActionSource = base.Source;
			return this;
		}

		// Token: 0x1700051F RID: 1311
		// (get) Token: 0x06000EFA RID: 3834 RVA: 0x0002863A File Offset: 0x0002683A
		public override bool IsModified
		{
			get
			{
				return this.Args.IsModified;
			}
		}

		// Token: 0x17000520 RID: 1312
		// (get) Token: 0x06000EFB RID: 3835 RVA: 0x00028647 File Offset: 0x00026847
		public override string[] Modifiers
		{
			get
			{
				return this.Args.Modifiers;
			}
		}

		// Token: 0x17000521 RID: 1313
		// (get) Token: 0x06000EFC RID: 3836 RVA: 0x00028654 File Offset: 0x00026854
		public override CancelCause CancelCause
		{
			get
			{
				return this.Args.CancelCause;
			}
		}

		// Token: 0x17000522 RID: 1314
		// (get) Token: 0x06000EFD RID: 3837 RVA: 0x00028661 File Offset: 0x00026861
		public override bool IsCanceled
		{
			get
			{
				return this.Args.IsCanceled;
			}
		}

		// Token: 0x06000EFE RID: 3838 RVA: 0x0002866E File Offset: 0x0002686E
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}

		// Token: 0x06000EFF RID: 3839 RVA: 0x0002867B File Offset: 0x0002687B
		internal override IEnumerable<Phase> GetPhases()
		{
			PlayCardAction.<>c__DisplayClass22_0 CS$<>8__locals1 = new PlayCardAction.<>c__DisplayClass22_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.card = this.Args.Card;
			yield return base.CreateEventPhase<CardUsingEventArgs>("CardPlaying", this.Args, base.Battle.CardPlaying);
			if (this.Args.PlayTwice)
			{
				this._twiceTokenCard = this.Args.Card.CloneTwiceToken();
				this._twiceTokenCard.IsPlayTwiceToken = true;
				this._twiceTokenCard.PlayTwiceSourceCard = this.Args.Card;
			}
			yield return base.CreatePhase("CardPlay/Canceled", delegate
			{
				CS$<>8__locals1.<>4__this.Args.CanCancel = false;
				CS$<>8__locals1.card.PlayInTriggered = CS$<>8__locals1.card.Triggered;
				switch (CS$<>8__locals1.card.Zone)
				{
				case CardZone.None:
					CS$<>8__locals1.<>4__this.Battle.AddCardToFollowArea(CS$<>8__locals1.card);
					return;
				case CardZone.Draw:
				case CardZone.Hand:
				case CardZone.Discard:
				case CardZone.Exile:
					CS$<>8__locals1.<>4__this.Battle.MoveCardToFollowArea(CS$<>8__locals1.card);
					return;
				case CardZone.FollowArea:
					return;
				}
				throw new ArgumentOutOfRangeException();
			}, true);
			yield return base.CreatePhase("Precondition", delegate
			{
				CS$<>8__locals1.<>4__this._precondition = CS$<>8__locals1.<>4__this.Args.Card.Precondition();
				if (CS$<>8__locals1.<>4__this._precondition != null)
				{
					CS$<>8__locals1.<>4__this._precondition.Source = CS$<>8__locals1.<>4__this.Args.Card;
					CS$<>8__locals1.<>4__this.React(new InteractionAction(CS$<>8__locals1.<>4__this._precondition, false), null, default(ActionCause?));
				}
			}, false);
			CS$<>8__locals1.played = false;
			if (!CS$<>8__locals1.card.IsForbidden && CS$<>8__locals1.card.CanUse)
			{
				int? num = CS$<>8__locals1.card.Config.MoneyCost;
				if (num != null)
				{
					int valueOrDefault = num.GetValueOrDefault();
					if (base.Battle.GameRun.Money < valueOrDefault)
					{
						goto IL_0328;
					}
				}
				if (!CS$<>8__locals1.card.Summoned || CS$<>8__locals1.card.Loyalty >= CS$<>8__locals1.card.MinLoyaltyToUseSkill)
				{
					PlayCardAction.<>c__DisplayClass22_1 CS$<>8__locals2 = new PlayCardAction.<>c__DisplayClass22_1();
					CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
					CS$<>8__locals2.CS$<>8__locals1.played = true;
					num = CS$<>8__locals2.CS$<>8__locals1.card.Config.MoneyCost;
					if (num != null)
					{
						CS$<>8__locals2.mc = num.GetValueOrDefault();
						yield return base.CreatePhase("MoneyCost", delegate
						{
							CS$<>8__locals2.CS$<>8__locals1.<>4__this.Battle.React(new ConsumeMoneyAction(CS$<>8__locals2.mc), CS$<>8__locals2.CS$<>8__locals1.card, ActionCause.CardUse);
						}, false);
					}
					CS$<>8__locals2.damageActions = new List<DamageAction>();
					yield return base.CreatePhase("CardAction", delegate
					{
						if (CS$<>8__locals2.CS$<>8__locals1.card.CardType == CardType.Friend)
						{
							CS$<>8__locals2.CS$<>8__locals1.card.Summoning = !CS$<>8__locals2.CS$<>8__locals1.card.Summoned;
						}
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.ReTargeting();
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.Battle.React(new Reactor(CS$<>8__locals2.CS$<>8__locals1.card.GetActions(CS$<>8__locals2.CS$<>8__locals1.<>4__this.Args.Selector, CS$<>8__locals2.CS$<>8__locals1.<>4__this.Args.ConsumingMana, CS$<>8__locals2.CS$<>8__locals1.<>4__this._precondition, CS$<>8__locals2.CS$<>8__locals1.<>4__this.Args.Kicker, CS$<>8__locals2.CS$<>8__locals1.card.Summoning, CS$<>8__locals2.damageActions)), CS$<>8__locals2.CS$<>8__locals1.card, ActionCause.Card);
					}, false);
					if (CS$<>8__locals2.damageActions.Count > 0)
					{
						yield return base.CreatePhase("Statistics", delegate
						{
							CS$<>8__locals2.CS$<>8__locals1.<>4__this.Battle.React(new StatisticalTotalDamageAction(CS$<>8__locals2.damageActions), CS$<>8__locals2.CS$<>8__locals1.<>4__this.Args.Card, ActionCause.Card);
						}, false);
					}
					CS$<>8__locals2 = null;
				}
			}
			IL_0328:
			yield return base.CreatePhase("AfterPlay", delegate
			{
				if (CS$<>8__locals1.played)
				{
					IEnumerable<BattleAction> enumerable = CS$<>8__locals1.<>4__this.Args.Card.AfterFollowPlayAction();
					if (enumerable != null)
					{
						CS$<>8__locals1.<>4__this.Battle.React(new Reactor(enumerable), CS$<>8__locals1.card, ActionCause.CardUse);
						return;
					}
				}
				else if (CS$<>8__locals1.<>4__this.Args.Card.IsPlayTwiceToken)
				{
					IEnumerable<BattleAction> enumerable2 = CS$<>8__locals1.<>4__this.Args.Card.AfterFollowPlayAction();
					if (enumerable2 != null)
					{
						CS$<>8__locals1.<>4__this.Battle.React(new Reactor(enumerable2), CS$<>8__locals1.card, ActionCause.CardUse);
						return;
					}
				}
				else
				{
					MoveCardAction moveCardAction = new MoveCardAction(CS$<>8__locals1.<>4__this.Args.Card, CardZone.Discard);
					CS$<>8__locals1.<>4__this.Battle.React(new Reactor(moveCardAction), CS$<>8__locals1.card, ActionCause.CardUse);
				}
			}, false);
			yield return base.CreatePhase("Record", delegate
			{
				CS$<>8__locals1.<>4__this.Battle.RecordCardPlay(CS$<>8__locals1.<>4__this.Args.Card);
			}, false);
			if (this.Args.PlayTwice && this._twiceTokenCard != null && !base.Battle.BattleShouldEnd)
			{
				yield return base.CreatePhase("PlayTwice", delegate
				{
					CS$<>8__locals1.<>4__this.React(new PlayTwiceAction(CS$<>8__locals1.<>4__this._twiceTokenCard, CS$<>8__locals1.<>4__this.Args), null, default(ActionCause?));
				}, false);
			}
			yield return base.CreateEventPhase<CardUsingEventArgs>("CardPlayed", this.Args, base.Battle.CardPlayed);
			yield break;
		}

		// Token: 0x06000F00 RID: 3840 RVA: 0x0002868C File Offset: 0x0002688C
		private void ReTargeting()
		{
			Card card = this.Args.Card;
			TargetType? targetType = card.Config.TargetType;
			TargetType targetType2 = TargetType.SingleEnemy;
			if ((targetType.GetValueOrDefault() == targetType2) & (targetType != null))
			{
				if (this.Args.Selector.Type == TargetType.SingleEnemy)
				{
					EnemyUnit selectedEnemy = this.Args.Selector.SelectedEnemy;
					if (selectedEnemy != null && selectedEnemy.IsAlive)
					{
						this.Args.Selector = new UnitSelector(this.Args.Selector.SelectedEnemy);
						return;
					}
				}
				this.Args.Selector = new UnitSelector(base.Battle.RandomAliveEnemy);
				return;
			}
			CardUsingEventArgs args = this.Args;
			targetType = card.Config.TargetType;
			if (targetType != null)
			{
				UnitSelector unitSelector;
				switch (targetType.GetValueOrDefault())
				{
				case TargetType.Nobody:
					unitSelector = UnitSelector.Nobody;
					break;
				case TargetType.SingleEnemy:
					goto IL_010C;
				case TargetType.AllEnemies:
					unitSelector = UnitSelector.AllEnemies;
					break;
				case TargetType.RandomEnemy:
					unitSelector = UnitSelector.RandomEnemy;
					break;
				case TargetType.Self:
					unitSelector = UnitSelector.Self;
					break;
				case TargetType.All:
					unitSelector = UnitSelector.All;
					break;
				default:
					goto IL_010C;
				}
				args.Selector = unitSelector;
				return;
			}
			IL_010C:
			throw new ArgumentOutOfRangeException();
		}

		// Token: 0x06000F01 RID: 3841 RVA: 0x000287B4 File Offset: 0x000269B4
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}

		// Token: 0x04000691 RID: 1681
		private Interaction _precondition;

		// Token: 0x04000692 RID: 1682
		private Card _twiceTokenCard;
	}
}
