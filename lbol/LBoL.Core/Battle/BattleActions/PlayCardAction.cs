using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class PlayCardAction : BattleAction
	{
		public CardUsingEventArgs Args { get; }
		public CardZone SourceZone { get; }
		public PlayCardAction(Card card)
		{
			this.Args = new CardUsingEventArgs
			{
				Card = card,
				Selector = UnitSelector.Nobody
			};
			this.SourceZone = card.Zone;
		}
		public PlayCardAction(Card card, UnitSelector selector)
		{
			this.Args = new CardUsingEventArgs
			{
				Card = card,
				Selector = selector
			};
			this.SourceZone = card.Zone;
		}
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
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.Args.Cause = cause;
			return this;
		}
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			this.Args.ActionSource = base.Source;
			return this;
		}
		public override bool IsModified
		{
			get
			{
				return this.Args.IsModified;
			}
		}
		public override string[] Modifiers
		{
			get
			{
				return this.Args.Modifiers;
			}
		}
		public override CancelCause CancelCause
		{
			get
			{
				return this.Args.CancelCause;
			}
		}
		public override bool IsCanceled
		{
			get
			{
				return this.Args.IsCanceled;
			}
		}
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}
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
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
		private Interaction _precondition;
		private Card _twiceTokenCard;
	}
}
