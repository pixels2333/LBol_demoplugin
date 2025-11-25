using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class EndPlayerTurnAction : BattleAction
	{
		public UnitEventArgs Args { get; }
		public Unit Unit { get; }
		internal EndPlayerTurnAction(Unit unit)
		{
			this.Unit = unit;
			this.Args = new UnitEventArgs
			{
				Unit = unit,
				CanCancel = false
			};
		}
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
				return Array.Empty<string>();
			}
		}
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
		public override CancelCause CancelCause
		{
			get
			{
				return CancelCause.None;
			}
		}
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
	}
}
