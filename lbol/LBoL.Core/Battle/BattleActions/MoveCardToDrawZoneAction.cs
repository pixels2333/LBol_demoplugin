using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
namespace LBoL.Core.Battle.BattleActions
{
	public class MoveCardToDrawZoneAction : EventBattleAction<CardMovingToDrawZoneEventArgs>
	{
		public CardTransitionType TransitionType { get; private set; }
		public MoveCardToDrawZoneAction([NotNull] Card card, DrawZoneTarget target)
		{
			base.Args = new CardMovingToDrawZoneEventArgs
			{
				Card = card,
				SourceZone = card.Zone,
				DrawZoneTarget = target
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			MoveCardToDrawZoneAction.<>c__DisplayClass5_0 CS$<>8__locals1 = new MoveCardToDrawZoneAction.<>c__DisplayClass5_0();
			CS$<>8__locals1.<>4__this = this;
			yield return base.CreateEventPhase<CardMovingToDrawZoneEventArgs>("Moving", base.Args, base.Battle.CardMovingToDrawZone);
			CS$<>8__locals1.srcZone = base.Args.Card.Zone;
			CS$<>8__locals1.reactor = null;
			yield return base.CreatePhase("Main", delegate
			{
				if (CS$<>8__locals1.<>4__this.Args.IsCanceled)
				{
					return;
				}
				CancelCause cancelCause = CS$<>8__locals1.<>4__this.Battle.MoveCardToDrawZone(CS$<>8__locals1.<>4__this.Args.Card, CS$<>8__locals1.<>4__this.Args.DrawZoneTarget);
				if (cancelCause != CancelCause.None)
				{
					CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(cancelCause);
					return;
				}
				CS$<>8__locals1.reactor = CS$<>8__locals1.<>4__this.Args.Card.OnMove(CS$<>8__locals1.srcZone, CardZone.Draw);
				if (CS$<>8__locals1.reactor != null && CS$<>8__locals1.<>4__this.Args.Card.OnMoveVisual)
				{
					CS$<>8__locals1.<>4__this.TransitionType = CardTransitionType.SpecialBegin;
				}
			}, true);
			if (CS$<>8__locals1.reactor != null)
			{
				MoveCardToDrawZoneAction.<>c__DisplayClass5_1 CS$<>8__locals2 = new MoveCardToDrawZoneAction.<>c__DisplayClass5_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				CS$<>8__locals2.damageActions = new List<DamageAction>();
				yield return base.CreatePhase("SpecialMove", delegate
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
				if (base.Args.Card.OnMoveVisual)
				{
					this.TransitionType = CardTransitionType.SpecialEnd;
					yield return base.CreatePhase("SpecialMoved", delegate
					{
					}, true);
				}
				CS$<>8__locals2 = null;
			}
			yield return base.CreateEventPhase<CardMovingToDrawZoneEventArgs>("Moved", base.Args, base.Battle.CardMovedToDrawZone);
			yield break;
		}
	}
}
