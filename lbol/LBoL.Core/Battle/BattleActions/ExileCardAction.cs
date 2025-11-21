using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Core.Exhibits;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200017E RID: 382
	public sealed class ExileCardAction : EventBattleAction<CardEventArgs>
	{
		// Token: 0x17000502 RID: 1282
		// (get) Token: 0x06000E74 RID: 3700 RVA: 0x00027644 File Offset: 0x00025844
		// (set) Token: 0x06000E75 RID: 3701 RVA: 0x0002764C File Offset: 0x0002584C
		public CardTransitionType TransitionType { get; private set; }

		// Token: 0x17000503 RID: 1283
		// (get) Token: 0x06000E76 RID: 3702 RVA: 0x00027655 File Offset: 0x00025855
		// (set) Token: 0x06000E77 RID: 3703 RVA: 0x0002765D File Offset: 0x0002585D
		public CardZone SourceZone { get; private set; }

		// Token: 0x17000504 RID: 1284
		// (get) Token: 0x06000E78 RID: 3704 RVA: 0x00027666 File Offset: 0x00025866
		// (set) Token: 0x06000E79 RID: 3705 RVA: 0x0002766E File Offset: 0x0002586E
		public ExileManyCardAction ManyCardAction { get; private set; }

		// Token: 0x06000E7A RID: 3706 RVA: 0x00027677 File Offset: 0x00025877
		public ExileCardAction(Card card)
		{
			base.Args = new CardEventArgs
			{
				Card = card
			};
		}

		// Token: 0x06000E7B RID: 3707 RVA: 0x00027691 File Offset: 0x00025891
		public ExileCardAction(Card card, ExileManyCardAction action)
		{
			base.Args = new CardEventArgs
			{
				Card = card
			};
			this.ManyCardAction = action;
		}

		// Token: 0x06000E7C RID: 3708 RVA: 0x000276B2 File Offset: 0x000258B2
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
