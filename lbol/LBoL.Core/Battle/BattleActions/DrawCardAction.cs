using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using UnityEngine;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class DrawCardAction : EventBattleAction<CardEventArgs>
	{
		public CardTransitionType TransitionType { get; private set; }
		public DrawCardAction()
		{
			base.Args = new CardEventArgs();
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			DrawCardAction.<>c__DisplayClass5_0 CS$<>8__locals1 = new DrawCardAction.<>c__DisplayClass5_0();
			CS$<>8__locals1.<>4__this = this;
			yield return base.CreateEventPhase<CardEventArgs>("Predraw", base.Args, base.Battle.Predraw);
			if (base.Battle.HandZone.Count == base.Battle.MaxHand)
			{
				while (base.Battle.HandZone.Count == base.Battle.MaxHand)
				{
					if (!Enumerable.Any<Card>(base.Battle.HandZone, (Card card) => card.IsAutoExile))
					{
						break;
					}
					Card firstAutoExile2 = Enumerable.FirstOrDefault<Card>(base.Battle.HandZone, (Card card) => card.IsAutoExile);
					if (firstAutoExile2 != null)
					{
						yield return base.CreatePhase("AutoExile", delegate
						{
							CS$<>8__locals1.<>4__this.Battle.React(new ExileCardAction(firstAutoExile2), CS$<>8__locals1.<>4__this.Args.Card, ActionCause.AutoExile);
						}, false);
					}
				}
				if (base.Battle.HandZone.Count == base.Battle.MaxHand)
				{
					yield return base.CreatePhase("HandFull", delegate
					{
						CS$<>8__locals1.<>4__this.Battle.NotifyMessage(BattleMessage.HandFull);
						CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(CancelCause.HandFull);
					}, false);
				}
			}
			if (base.Battle.DrawZone.Empty<Card>() && !base.Battle.DiscardZone.Empty<Card>())
			{
				yield return base.CreatePhase("Reshuffle", delegate
				{
					CS$<>8__locals1.<>4__this.Battle.React(new ReshuffleAction(), null, ActionCause.None);
				}, false);
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
			}
			if (base.Battle.DrawZone.Empty<Card>())
			{
				yield return base.CreatePhase("EmptyDraw", delegate
				{
					CS$<>8__locals1.<>4__this.Battle.NotifyMessage(BattleMessage.EmptyDraw);
					CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(CancelCause.EmptyDraw);
				}, false);
			}
			base.Args.Card = base.Battle.DrawZone[0];
			base.Args.IsModified = true;
			base.Args.CanCancel = false;
			yield return base.CreateEventPhase<CardEventArgs>("CardDrawing", base.Args, base.Battle.CardDrawing);
			CS$<>8__locals1.reactor = null;
			yield return base.CreatePhase("Main", delegate
			{
				Card card = CS$<>8__locals1.<>4__this.Battle.DrawCard();
				if (card != CS$<>8__locals1.<>4__this.Args.Card)
				{
					Debug.LogError("Drawing card is modified, drawing " + CS$<>8__locals1.<>4__this.Args.Card.DebugName + ", drawn " + (((card != null) ? card.DebugName : null) ?? "<null>"));
					CS$<>8__locals1.<>4__this.Args.Card = card;
				}
				if (CS$<>8__locals1.<>4__this.Args.Card == null)
				{
					CS$<>8__locals1.<>4__this.Args.ForceCancelBecause(CancelCause.Failure);
					return;
				}
				CS$<>8__locals1.reactor = CS$<>8__locals1.<>4__this.Args.Card.OnDraw();
				if (CS$<>8__locals1.reactor != null && CS$<>8__locals1.<>4__this.Args.Card.OnDrawVisual)
				{
					CS$<>8__locals1.<>4__this.TransitionType = CardTransitionType.SpecialBegin;
				}
			}, true);
			if (CS$<>8__locals1.reactor != null)
			{
				DrawCardAction.<>c__DisplayClass5_3 CS$<>8__locals4 = new DrawCardAction.<>c__DisplayClass5_3();
				CS$<>8__locals4.CS$<>8__locals3 = CS$<>8__locals1;
				CS$<>8__locals4.damageActions = new List<DamageAction>();
				yield return base.CreatePhase("SpecialDraw", delegate
				{
					CS$<>8__locals4.CS$<>8__locals3.<>4__this.React(new Reactor(StatisticalTotalDamageAction.WrapReactorWithStats(CS$<>8__locals4.CS$<>8__locals3.reactor, CS$<>8__locals4.damageActions)), CS$<>8__locals4.CS$<>8__locals3.<>4__this.Args.Card, new ActionCause?(ActionCause.Card));
				}, false);
				if (CS$<>8__locals4.damageActions.NotEmpty<DamageAction>())
				{
					yield return base.CreatePhase("Statistics", delegate
					{
						CS$<>8__locals4.CS$<>8__locals3.<>4__this.Battle.React(new StatisticalTotalDamageAction(CS$<>8__locals4.damageActions), CS$<>8__locals4.CS$<>8__locals3.<>4__this.Args.Card, ActionCause.Card);
					}, false);
				}
				if (base.Args.Card.OnDrawVisual)
				{
					this.TransitionType = CardTransitionType.SpecialEnd;
					yield return base.CreatePhase("SpecialDrawn", delegate
					{
					}, true);
				}
				CS$<>8__locals4 = null;
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
