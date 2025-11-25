using System;
using System.Collections.Generic;
using System.Linq;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class StartPlayerTurnAction : SimpleAction
	{
		public UnitEventArgs Args { get; }
		public Unit Unit { get; }
		public bool IsExtra { get; }
		internal StartPlayerTurnAction(Unit unit, bool isExtra)
		{
			this.Unit = unit;
			this.IsExtra = isExtra;
			this.Args = new UnitEventArgs
			{
				Unit = unit,
				CanCancel = false
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Viewer", delegate
			{
			}, true);
			yield return base.CreatePhase("LoseBlockGraze", delegate
			{
				SuperExtraTurn statusEffect = this.Unit.GetStatusEffect<SuperExtraTurn>();
				if (statusEffect != null && statusEffect.IsInExtraTurnByThis)
				{
					return;
				}
				TurnStartDontLoseBlock statusEffect2 = this.Unit.GetStatusEffect<TurnStartDontLoseBlock>();
				if (statusEffect2 != null)
				{
					TurnStartDontLoseBlock turnStartDontLoseBlock = statusEffect2;
					int num = turnStartDontLoseBlock.Level - 1;
					turnStartDontLoseBlock.Level = num;
					if (statusEffect2.Level == 0)
					{
						base.React(new RemoveStatusEffectAction(statusEffect2, true, 0.1f), null, default(ActionCause?));
					}
				}
				else
				{
					base.React(new LoseBlockShieldAction(this.Unit, this.Unit.Block, 0, false), null, new ActionCause?(ActionCause.TurnStart));
				}
				if (!this.Unit.IsExtraTurn && !this.Unit.HasStatusEffect<WindGirl>())
				{
					Graze statusEffect3 = this.Unit.GetStatusEffect<Graze>();
					if (statusEffect3 == null)
					{
						return;
					}
					statusEffect3.LoseGraze();
				}
			}, false);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnStarting", this.Args, base.Battle.Player.TurnStarting);
			yield return base.CreatePhase("DecreaseDuration", delegate
			{
				base.Battle.TurnStartDecreaseDuration(base.Battle.Player);
			}, false);
			yield return base.CreatePhase("Main", delegate
			{
				base.Battle.StartPlayerTurn();
			}, false);
			base.Battle.StartTurnDrawing = false;
			List<Card> list = Enumerable.ToList<Card>(base.Battle.HandZone);
			using (List<Card>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					StartPlayerTurnAction.<>c__DisplayClass10_0 CS$<>8__locals1 = new StartPlayerTurnAction.<>c__DisplayClass10_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.card = enumerator.Current;
					if (CS$<>8__locals1.card.Zone == CardZone.Hand)
					{
						CS$<>8__locals1.reactor = CS$<>8__locals1.card.OnTurnStartedInHand();
						if (CS$<>8__locals1.reactor != null)
						{
							CS$<>8__locals1.damageActions = new List<DamageAction>();
							yield return base.CreatePhase("SpecialHandInTurnStarted", delegate
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
			base.Battle.PlayerSummonAFriendThisTurn = false;
			yield return base.CreateEventPhase<UnitEventArgs>("TurnStarted", this.Args, base.Battle.Player.TurnStarted);
			yield return base.CreatePhase("InTurn", delegate
			{
				base.Battle.Player.IsInTurn = true;
			}, false);
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
				return this.Args.Modifiers;
			}
		}
		public override bool IsCanceled
		{
			get
			{
				return false;
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
