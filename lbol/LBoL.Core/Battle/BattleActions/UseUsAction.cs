using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class UseUsAction : BattleAction
	{
		public UsUsingEventArgs Args { get; }
		public UseUsAction(UltimateSkill us, UnitSelector selector, int consumingEnergy)
		{
			this.Args = new UsUsingEventArgs
			{
				Us = us,
				Selector = selector,
				ConsumingPower = consumingEnergy
			};
		}
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.Args.Cause = cause;
			return this;
		}
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			this.Args.ActionSource = base.Source;
			return this;
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
				return this.Args.IsCanceled;
			}
		}
		public override CancelCause CancelCause
		{
			get
			{
				return this.Args.CancelCause;
			}
		}
		public override void ClearModifiers()
		{
			this.Args.ClearModifiers();
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreateEventPhase<UsUsingEventArgs>("UsUsing", this.Args, base.Battle.UsUsing);
			yield return base.CreatePhase("UsUsing", delegate
			{
				this.Args.CanCancel = false;
				if (!this.Args.Us.TurnRepeatable)
				{
					this.Args.Us.TurnAvailable = false;
				}
				if (!this.Args.Us.BattleRepeatable)
				{
					this.Args.Us.BattleAvailable = false;
				}
				this.Battle.React(new ConsumePowerAction(this.Args.ConsumingPower), this.Args.Us, ActionCause.UsUse);
			}, false);
			List<DamageAction> damageActions = new List<DamageAction>();
			yield return base.CreatePhase("UsAction", delegate
			{
				this.Battle.React(new Reactor(this.Args.Us.GetActions(this.Args.Selector, damageActions)), this.Args.Us, ActionCause.Us);
			}, false);
			if (damageActions.Count > 0)
			{
				yield return base.CreatePhase("Statistics", delegate
				{
					this.Battle.React(new StatisticalTotalDamageAction(damageActions), this.Args.Us, ActionCause.Us);
				}, false);
			}
			base.Battle.GameRun.UltimateUseCount++;
			yield return base.CreateEventPhase<UsUsingEventArgs>("UsUsed", this.Args, base.Battle.UsUsed);
			yield break;
		}
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
	}
}
