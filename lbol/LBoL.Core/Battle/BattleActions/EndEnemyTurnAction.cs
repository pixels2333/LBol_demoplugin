using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class EndEnemyTurnAction : BattleAction
	{
		public UnitEventArgs Args { get; }
		public EnemyUnit Unit { get; }
		internal EndEnemyTurnAction(EnemyUnit enemy)
		{
			this.Unit = enemy;
			this.Args = new UnitEventArgs
			{
				Unit = enemy,
				CanCancel = false
			};
		}
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("OutTurn", delegate
			{
				this.Unit.IsInTurn = false;
			}, false);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnEnding", this.Args, this.Args.Unit.TurnEnding);
			yield return base.CreatePhase("Main", delegate
			{
				base.Battle.EndEnemyTurn(this.Unit);
			}, true);
			yield return base.CreatePhase("DecreaseDuration", delegate
			{
				base.Battle.TurnEndDecreaseDuration(this.Unit);
			}, false);
			yield return base.CreateEventPhase<UnitEventArgs>("TurnEnded", this.Args, this.Args.Unit.TurnEnded);
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
