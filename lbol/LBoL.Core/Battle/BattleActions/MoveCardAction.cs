using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000194 RID: 404
	public class MoveCardAction : EventBattleAction<CardMovingEventArgs>
	{
		// Token: 0x17000519 RID: 1305
		// (get) Token: 0x06000ED8 RID: 3800 RVA: 0x00028290 File Offset: 0x00026490
		// (set) Token: 0x06000ED9 RID: 3801 RVA: 0x00028298 File Offset: 0x00026498
		public CardTransitionType TransitionType { get; private set; }

		// Token: 0x1700051A RID: 1306
		// (get) Token: 0x06000EDA RID: 3802 RVA: 0x000282A1 File Offset: 0x000264A1
		// (set) Token: 0x06000EDB RID: 3803 RVA: 0x000282A9 File Offset: 0x000264A9
		public DreamCardsAction DreamCardsAction { get; internal set; }

		// Token: 0x06000EDC RID: 3804 RVA: 0x000282B2 File Offset: 0x000264B2
		public MoveCardAction(Card card, CardZone dstZone)
		{
			BattleController.MoveCardCheck(card, dstZone);
			base.Args = new CardMovingEventArgs
			{
				Card = card,
				SourceZone = card.Zone,
				DestinationZone = dstZone
			};
		}

		// Token: 0x06000EDD RID: 3805 RVA: 0x000282E6 File Offset: 0x000264E6
		internal override IEnumerable<Phase> GetPhases()
		{
			MoveCardAction.<>c__DisplayClass9_0 CS$<>8__locals1 = new MoveCardAction.<>c__DisplayClass9_0();
			CS$<>8__locals1.<>4__this = this;
			if (base.Args.DestinationZone == CardZone.Hand && base.Battle.HandZone.Count == base.Battle.MaxHand)
			{
				while (base.Battle.HandZone.Count == base.Battle.MaxHand)
				{
					if (!Enumerable.Any<Card>(base.Battle.HandZone, (Card card) => card.IsAutoExile))
					{
						break;
					}
					Card firstAutoExile = Enumerable.FirstOrDefault<Card>(base.Battle.HandZone, (Card card) => card.IsAutoExile);
					if (firstAutoExile != null)
					{
						yield return base.CreatePhase("AutoExile", delegate
						{
							CS$<>8__locals1.<>4__this.Battle.React(new ExileCardAction(firstAutoExile), CS$<>8__locals1.<>4__this.Args.Card, ActionCause.AutoExile);
						}, false);
					}
				}
			}
			yield return base.CreateEventPhase<CardMovingEventArgs>("Moving", base.Args, base.Battle.CardMoving);
			CS$<>8__locals1.srcZone = base.Args.Card.Zone;
			CS$<>8__locals1.reactor = null;
			yield return base.CreatePhase("Main", delegate
			{
				if (CS$<>8__locals1.<>4__this.Args.IsCanceled)
				{
					return;
				}
				CancelCause cancelCause = CS$<>8__locals1.<>4__this.Battle.MoveCard(CS$<>8__locals1.<>4__this.Args.Card, CS$<>8__locals1.<>4__this.Args.DestinationZone);
				if (cancelCause != CancelCause.None)
				{
					CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(cancelCause);
					return;
				}
				CS$<>8__locals1.reactor = CS$<>8__locals1.<>4__this.Args.Card.OnMove(CS$<>8__locals1.srcZone, CS$<>8__locals1.<>4__this.Args.DestinationZone);
				if (CS$<>8__locals1.reactor != null && CS$<>8__locals1.<>4__this.Args.Card.OnMoveVisual)
				{
					CS$<>8__locals1.<>4__this.TransitionType = CardTransitionType.SpecialBegin;
				}
			}, true);
			if (CS$<>8__locals1.reactor != null)
			{
				MoveCardAction.<>c__DisplayClass9_2 CS$<>8__locals3 = new MoveCardAction.<>c__DisplayClass9_2();
				CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals1;
				CS$<>8__locals3.damageActions = new List<DamageAction>();
				yield return base.CreatePhase("SpecialMove", delegate
				{
					CS$<>8__locals3.CS$<>8__locals2.<>4__this.React(new Reactor(StatisticalTotalDamageAction.WrapReactorWithStats(CS$<>8__locals3.CS$<>8__locals2.reactor, CS$<>8__locals3.damageActions)), CS$<>8__locals3.CS$<>8__locals2.<>4__this.Args.Card, new ActionCause?(ActionCause.Card));
				}, false);
				if (CS$<>8__locals3.damageActions.NotEmpty<DamageAction>())
				{
					yield return base.CreatePhase("Statistics", delegate
					{
						CS$<>8__locals3.CS$<>8__locals2.<>4__this.Battle.React(new StatisticalTotalDamageAction(CS$<>8__locals3.damageActions), CS$<>8__locals3.CS$<>8__locals2.<>4__this.Args.Card, ActionCause.Card);
					}, false);
				}
				if (base.Args.Card.OnMoveVisual)
				{
					this.TransitionType = CardTransitionType.SpecialEnd;
					yield return base.CreatePhase("SpecialMoved", delegate
					{
					}, true);
				}
				CS$<>8__locals3 = null;
			}
			yield return base.CreateEventPhase<CardMovingEventArgs>("Moved", base.Args, base.Battle.CardMoved);
			yield break;
		}
	}
}
