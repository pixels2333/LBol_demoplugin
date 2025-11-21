using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000178 RID: 376
	public sealed class EndPlayerTurnAction : BattleAction
	{
		// Token: 0x170004F7 RID: 1271
		// (get) Token: 0x06000E54 RID: 3668 RVA: 0x0002731C File Offset: 0x0002551C
		public UnitEventArgs Args { get; }

		// Token: 0x170004F8 RID: 1272
		// (get) Token: 0x06000E55 RID: 3669 RVA: 0x00027324 File Offset: 0x00025524
		public Unit Unit { get; }

		// Token: 0x06000E56 RID: 3670 RVA: 0x0002732C File Offset: 0x0002552C
		internal EndPlayerTurnAction(Unit unit)
		{
			this.Unit = unit;
			this.Args = new UnitEventArgs
			{
				Unit = unit,
				CanCancel = false
			};
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x00027354 File Offset: 0x00025554
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("OutTurn", delegate
			{
				base.Battle.Player.IsInTurn = false;
			}, false);
			List<Card> list = Enumerable.ToList<Card>(base.Battle.HandZone);
			using (List<Card>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					EndPlayerTurnAction.<>c__DisplayClass7_0 CS$<>8__locals1 = new EndPlayerTurnAction.<>c__DisplayClass7_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.card = enumerator.Current;
					if (CS$<>8__locals1.card.Zone == CardZone.Hand)
					{
						CS$<>8__locals1.reactor = CS$<>8__locals1.card.OnTurnEndingInHand();
						if (CS$<>8__locals1.reactor != null)
						{
							CS$<>8__locals1.damageActions = new List<DamageAction>();
							yield return base.CreatePhase("SpecialHandInTurnEnding", delegate
							{
								CS$<>8__locals1.<>4__this.React(new Reactor(StatisticalTotalDamageAction.WrapReactorWithStats(CS$<>8__locals1.reactor, CS$<>8__locals1.damageActions)), CS$<>8__locals1.card, new ActionCause?(ActionCause.Card));
							}, false);
							if (CS$<>8__locals1.damageActions.NotEmpty<DamageAction>())
							{
								yield return base.CreatePhase("Statistics", delegate
								{
									CS$<>8__locals1.<>4__this.Battle.React(new StatisticalTotalDamageAction(CS$<>8__locals1.damageActions), CS$<>8__locals1.card, ActionCause.Card);
								}, false);
							}
						}
					}
					CS$<>8__locals1 = null;
				}
			}
			List<Card>.Enumerator enumerator = default(List<Card>.Enumerator);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnEnding", this.Args, base.Battle.Player.TurnEnding);
			yield return base.CreatePhase("Main", new Action(base.Battle.EndPlayerTurn), true);
			if (base.Battle.DrawAfterDiscard > 0)
			{
				int draw = base.Battle.DrawAfterDiscard;
				base.Battle.DrawAfterDiscard = 0;
				yield return base.CreatePhase("SpecialDrawInTurnEnded", delegate
				{
					this.Battle.React(new DrawManyCardAction(draw), null, ActionCause.TurnEnd);
				}, false);
			}
			yield return base.CreatePhase("DecreaseDuration", delegate
			{
				base.Battle.TurnEndDecreaseDuration(this.Unit);
			}, false);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnEnded", this.Args, base.Battle.Player.TurnEnded);
			using (IEnumerator<Card> enumerator2 = base.Battle.EnumerateAllCards().GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					Card card = enumerator2.Current;
					if (card.IsPlentiful)
					{
						card.PlentifulHappenThisTurn = false;
					}
				}
				yield break;
			}
			yield break;
			yield break;
		}

		// Token: 0x170004F9 RID: 1273
		// (get) Token: 0x06000E58 RID: 3672 RVA: 0x00027364 File Offset: 0x00025564
		public override bool IsModified
		{
			get
			{
				return this.Args.IsModified;
			}
		}

		// Token: 0x170004FA RID: 1274
		// (get) Token: 0x06000E59 RID: 3673 RVA: 0x00027371 File Offset: 0x00025571
		public override string[] Modifiers
		{
			get
			{
				return Array.Empty<string>();
			}
		}

		// Token: 0x170004FB RID: 1275
		// (get) Token: 0x06000E5A RID: 3674 RVA: 0x00027378 File Offset: 0x00025578
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004FC RID: 1276
		// (get) Token: 0x06000E5B RID: 3675 RVA: 0x0002737B File Offset: 0x0002557B
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x0002737E File Offset: 0x0002557E
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}

		// Token: 0x06000E5D RID: 3677 RVA: 0x0002738B File Offset: 0x0002558B
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
	}
}
