using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Core.Exhibits;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class ExileCardAction : EventBattleAction<CardEventArgs>
	{
		public CardTransitionType TransitionType { get; private set; }
		public CardZone SourceZone { get; private set; }
		public ExileManyCardAction ManyCardAction { get; private set; }
		public ExileCardAction(Card card)
		{
			base.Args = new CardEventArgs
			{
				Card = card
			};
		}
		public ExileCardAction(Card card, ExileManyCardAction action)
		{
			base.Args = new CardEventArgs
			{
				Card = card
			};
			this.ManyCardAction = action;
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			ExileCardAction.<>c__DisplayClass14_0 CS$<>8__locals1 = new ExileCardAction.<>c__DisplayClass14_0();
			CS$<>8__locals1.<>4__this = this;
			this.SourceZone = base.Args.Card.Zone;
			if (this.SourceZone == CardZone.Exile)
			{
				base.Args.ForceCancelBecause(CancelCause.AlreadyExiled);
			}
			yield return base.CreateEventPhase<CardEventArgs>("Exiling", base.Args, base.Battle.CardExiling);
			CS$<>8__locals1.reactor = null;
			yield return base.CreatePhase("Main", delegate
			{
				if (CS$<>8__locals1.<>4__this.Args.IsCanceled)
				{
					return;
				}
				CancelCause cancelCause = CS$<>8__locals1.<>4__this.Battle.ExileCard(CS$<>8__locals1.<>4__this.Args.Card);
				if (cancelCause != CancelCause.None)
				{
					CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(cancelCause);
					return;
				}
				CS$<>8__locals1.reactor = CS$<>8__locals1.<>4__this.Args.Card.OnExile(CS$<>8__locals1.<>4__this.SourceZone);
				if (CS$<>8__locals1.reactor != null && CS$<>8__locals1.<>4__this.Args.Card.OnExileVisual)
				{
					CS$<>8__locals1.<>4__this.TransitionType = CardTransitionType.SpecialBegin;
				}
			}, true);
			if (CS$<>8__locals1.reactor != null)
			{
				ExileCardAction.<>c__DisplayClass14_1 CS$<>8__locals2 = new ExileCardAction.<>c__DisplayClass14_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CS$<>8__locals2.damageActions = new List<DamageAction>();
				yield return base.CreatePhase("SpecialExile", delegate
				{
					CS$<>8__locals2.CS$<>8__locals1.<>4__this.React(new Reactor(StatisticalTotalDamageAction.WrapReactorWithStats(CS$<>8__locals2.CS$<>8__locals1.reactor, CS$<>8__locals2.damageActions)), CS$<>8__locals2.CS$<>8__locals1.<>4__this.Args.Card, new ActionCause?(ActionCause.Card));
				}, false);
				if (CS$<>8__locals2.damageActions.NotEmpty<DamageAction>())
				{
					yield return base.CreatePhase("Statistics", delegate
					{
						CS$<>8__locals2.CS$<>8__locals1.<>4__this.Battle.React(new StatisticalTotalDamageAction(CS$<>8__locals2.damageActions), CS$<>8__locals2.CS$<>8__locals1.<>4__this.Args.Card, ActionCause.Card);
					}, false);
				}
				if (base.Args.Card.OnExileVisual)
				{
					this.TransitionType = CardTransitionType.SpecialEnd;
					yield return base.CreatePhase("SpecialExiled", delegate
					{
					}, true);
				}
				CS$<>8__locals2 = null;
			}
			yield return base.CreateEventPhase<CardEventArgs>("Exiled", base.Args, base.Battle.CardExiled);
			if (base.Battle.Player.IsInTurn && base.Battle.GameRun.YichuiPiaoFlag > 0 && base.Battle.HandZone.Count == 0)
			{
				yield return base.CreatePhase("AfterExile", delegate
				{
					YichuiPiao exhibit = CS$<>8__locals1.<>4__this.Battle.Player.GetExhibit<YichuiPiao>();
					exhibit.NotifyActivating();
					CS$<>8__locals1.<>4__this.Battle.React(new DrawCardsToSpecificAction(1), exhibit, ActionCause.Exhibit);
				}, false);
			}
			yield break;
		}
	}
}
