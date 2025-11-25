using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class DiscardAction : EventBattleAction<CardEventArgs>
	{
		public CardTransitionType TransitionType { get; private set; }
		public CardZone SourceZone { get; private set; }
		public DiscardAction(Card card)
		{
			base.Args = new CardEventArgs
			{
				Card = card
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			DiscardAction.<>c__DisplayClass9_0 CS$<>8__locals1 = new DiscardAction.<>c__DisplayClass9_0();
			CS$<>8__locals1.<>4__this = this;
			yield return base.CreateEventPhase<CardEventArgs>("Discarding", base.Args, base.Battle.CardDiscarding);
			this.SourceZone = base.Args.Card.Zone;
			CS$<>8__locals1.reactor = null;
			yield return base.CreatePhase("Main", delegate
			{
				if (CS$<>8__locals1.<>4__this.Args.IsCanceled)
				{
					return;
				}
				CancelCause cancelCause = CS$<>8__locals1.<>4__this.Battle.Discard(CS$<>8__locals1.<>4__this.Args.Card);
				if (cancelCause != CancelCause.None)
				{
					CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(cancelCause);
					return;
				}
				CS$<>8__locals1.reactor = CS$<>8__locals1.<>4__this.Args.Card.OnDiscard(CS$<>8__locals1.<>4__this.SourceZone);
				if (CS$<>8__locals1.reactor != null && CS$<>8__locals1.<>4__this.Args.Card.OnDiscardVisual)
				{
					CS$<>8__locals1.<>4__this.TransitionType = CardTransitionType.SpecialBegin;
				}
			}, true);
			if (CS$<>8__locals1.reactor != null)
			{
				DiscardAction.<>c__DisplayClass9_1 CS$<>8__locals2 = new DiscardAction.<>c__DisplayClass9_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CS$<>8__locals2.damageActions = new List<DamageAction>();
				yield return base.CreatePhase("SpecialDiscard", delegate
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
				if (base.Args.Card.OnDiscardVisual)
				{
					this.TransitionType = CardTransitionType.SpecialEnd;
					yield return base.CreatePhase("SpecialDiscarded", delegate
					{
					}, true);
				}
				CS$<>8__locals2 = null;
			}
			yield return base.CreateEventPhase<CardEventArgs>("Discarded", base.Args, base.Battle.CardDiscarded);
			yield break;
		}
	}
}
