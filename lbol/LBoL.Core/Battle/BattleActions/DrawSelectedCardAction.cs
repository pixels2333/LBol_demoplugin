using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000172 RID: 370
	public sealed class DrawSelectedCardAction : EventBattleAction<CardEventArgs>
	{
		// Token: 0x170004EE RID: 1262
		// (get) Token: 0x06000E34 RID: 3636 RVA: 0x00027110 File Offset: 0x00025310
		// (set) Token: 0x06000E35 RID: 3637 RVA: 0x00027118 File Offset: 0x00025318
		public CardTransitionType TransitionType { get; private set; }

		// Token: 0x06000E36 RID: 3638 RVA: 0x00027121 File Offset: 0x00025321
		public DrawSelectedCardAction(Card card)
		{
			base.Args = new CardEventArgs
			{
				Card = card
			};
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x0002713B File Offset: 0x0002533B
		internal override IEnumerable<Phase> GetPhases()
		{
			DrawSelectedCardAction.<>c__DisplayClass5_0 CS$<>8__locals1 = new DrawSelectedCardAction.<>c__DisplayClass5_0();
			CS$<>8__locals1.<>4__this = this;
			if (base.Battle.HandZone.Count == base.Battle.MaxHand)
			{
				Card firstAutoExile = Enumerable.FirstOrDefault<Card>(base.Battle.HandZone, (Card card) => card.IsAutoExile);
				if (firstAutoExile != null)
				{
					yield return base.CreatePhase("AutoExile", delegate
					{
						CS$<>8__locals1.<>4__this.Battle.React(new ExileCardAction(firstAutoExile), CS$<>8__locals1.<>4__this.Args.Card, ActionCause.AutoExile);
					}, false);
				}
				else
				{
					yield return base.CreatePhase("HandFull", delegate
					{
						CS$<>8__locals1.<>4__this.Battle.NotifyMessage(BattleMessage.HandFull);
						CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(CancelCause.HandFull);
					}, false);
				}
			}
			yield return base.CreateEventPhase<CardEventArgs>("CardDrawing", base.Args, base.Battle.CardDrawing);
			CS$<>8__locals1.reactor = null;
			yield return base.CreatePhase("Main", delegate
			{
				CS$<>8__locals1.<>4__this.Args.Card = CS$<>8__locals1.<>4__this.Battle.DrawSelectedCard(CS$<>8__locals1.<>4__this.Args.Card);
				if (CS$<>8__locals1.<>4__this.Args.Card == null)
				{
					CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(CancelCause.Failure);
				}
				else
				{
					CS$<>8__locals1.<>4__this.Args.CanCancel = false;
					CS$<>8__locals1.reactor = CS$<>8__locals1.<>4__this.Args.Card.OnDraw();
					if (CS$<>8__locals1.reactor != null && CS$<>8__locals1.<>4__this.Args.Card.OnDrawVisual)
					{
						CS$<>8__locals1.<>4__this.TransitionType = CardTransitionType.SpecialBegin;
					}
				}
				CS$<>8__locals1.<>4__this.Args.IsModified = true;
			}, true);
			if (CS$<>8__locals1.reactor != null)
			{
				DrawSelectedCardAction.<>c__DisplayClass5_2 CS$<>8__locals3 = new DrawSelectedCardAction.<>c__DisplayClass5_2();
				CS$<>8__locals3.CS$<>8__locals2 = CS$<>8__locals1;
				CS$<>8__locals3.damageActions = new List<DamageAction>();
				yield return base.CreatePhase("SpecialDraw", delegate
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
				if (base.Args.Card.OnDrawVisual)
				{
					this.TransitionType = CardTransitionType.SpecialEnd;
					yield return base.CreatePhase("SpecialDrawn", delegate
					{
					}, true);
				}
				CS$<>8__locals3 = null;
			}
			if (!base.Args.IsCanceled && base.Args.Card.IsPlentiful && base.Args.Card.PlentifulMana != null && !base.Args.Card.PlentifulHappenThisTurn)
			{
				yield return base.CreatePhase("Plentiful", delegate
				{
					CS$<>8__locals1.<>4__this.Battle.React(new GainManaAction(CS$<>8__locals1.<>4__this.Args.Card.PlentifulMana.Value), CS$<>8__locals1.<>4__this.Args.Card, ActionCause.Card);
					CS$<>8__locals1.<>4__this.Args.Card.PlentifulHappenThisTurn = true;
				}, false);
			}
			if (!base.Args.IsCanceled && base.Args.Card.IsReplenish)
			{
				yield return base.CreatePhase("Replenish", delegate
				{
					CS$<>8__locals1.<>4__this.Battle.React(new DrawCardAction(), CS$<>8__locals1.<>4__this.Args.Card, ActionCause.Card);
				}, false);
			}
			yield return base.CreateEventPhase<CardEventArgs>("CardDrawn", base.Args, base.Battle.CardDrawn);
			yield break;
		}
	}
}
