using System;
using System.Collections.Generic;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001A0 RID: 416
	public sealed class RetainAction : EventBattleAction<CardEventArgs>
	{
		// Token: 0x17000524 RID: 1316
		// (get) Token: 0x06000F1C RID: 3868 RVA: 0x00028CBB File Offset: 0x00026EBB
		// (set) Token: 0x06000F1D RID: 3869 RVA: 0x00028CC3 File Offset: 0x00026EC3
		public CardTransitionType TransitionType { get; private set; }

		// Token: 0x06000F1E RID: 3870 RVA: 0x00028CCC File Offset: 0x00026ECC
		public RetainAction(Card card)
		{
			base.Args = new CardEventArgs
			{
				Card = card,
				CanCancel = false
			};
		}

		// Token: 0x06000F1F RID: 3871 RVA: 0x00028CED File Offset: 0x00026EED
		internal override IEnumerable<Phase> GetPhases()
		{
			RetainAction.<>c__DisplayClass5_0 CS$<>8__locals1 = new RetainAction.<>c__DisplayClass5_0();
			CS$<>8__locals1.<>4__this = this;
			yield return base.CreateEventPhase<CardEventArgs>("Retaining", base.Args, base.Battle.CardRetaining);
			CS$<>8__locals1.reactor = null;
			yield return base.CreatePhase("Main", delegate
			{
				CS$<>8__locals1.reactor = CS$<>8__locals1.<>4__this.Args.Card.OnRetain();
			}, true);
			if (CS$<>8__locals1.reactor != null)
			{
				RetainAction.<>c__DisplayClass5_1 CS$<>8__locals2 = new RetainAction.<>c__DisplayClass5_1();
				CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
				bool specialReacting = base.Args.Card.OnRetainVisual;
				if (specialReacting)
				{
					this.TransitionType = CardTransitionType.SpecialBegin;
				}
				CS$<>8__locals2.damageActions = new List<DamageAction>();
				yield return base.CreatePhase("SpecialRetain", delegate
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
				if (specialReacting)
				{
					this.TransitionType = CardTransitionType.SpecialEnd;
					yield return base.CreatePhase("SpecialRetained", delegate
					{
					}, true);
				}
				CS$<>8__locals2 = null;
			}
			yield return base.CreateEventPhase<CardEventArgs>("Retained", base.Args, base.Battle.CardRetained);
			yield break;
		}
	}
}
