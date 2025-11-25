using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class UseDollAction : BattleAction
	{
		public DollUsingEventArgs Args { get; }
		public UseDollAction(Doll doll, UnitSelector selector)
		{
			this.Args = new DollUsingEventArgs
			{
				Doll = doll,
				Selector = selector,
				ConsumingMagic = doll.MagicCost
			};
		}
		public UseDollAction(Doll doll, UnitSelector selector, int consumingMagic)
		{
			this.Args = new DollUsingEventArgs
			{
				Doll = doll,
				Selector = selector,
				ConsumingMagic = consumingMagic
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
			yield return base.CreateEventPhase<DollUsingEventArgs>("DollUsing", this.Args, base.Battle.DollUsing);
			yield return base.CreatePhase("DollUsing", delegate
			{
				this.Args.CanCancel = false;
				this.Battle.React(new ConsumeMagicAction(this.Args.Doll), this.Args.Doll, ActionCause.Doll);
			}, false);
			List<DamageAction> damageActions = new List<DamageAction>();
			yield return base.CreatePhase("DollAction", delegate
			{
				this.Battle.React(new Reactor(this.Args.Doll.GetActions(this.Args.Selector, damageActions)), this.Args.Doll, ActionCause.Doll);
			}, false);
			if (damageActions.Count > 0)
			{
				yield return base.CreatePhase("Statistics", delegate
				{
					this.Battle.React(new StatisticalTotalDamageAction(damageActions), this.Args.Doll, ActionCause.Doll);
				}, false);
			}
			yield return base.CreateEventPhase<DollUsingEventArgs>("DollUsed", this.Args, base.Battle.DollUsed);
			yield break;
		}
		public override string ExportDebugDetails()
		{
			return this.Args.ExportDebugDetails();
		}
	}
}
