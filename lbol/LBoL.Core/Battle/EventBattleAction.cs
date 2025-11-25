using System;
using UnityEngine;
namespace LBoL.Core.Battle
{
	public abstract class EventBattleAction<TArgs> : BattleAction where TArgs : GameEventArgs
	{
		public TArgs Args { get; internal set; }
		public override BattleAction SetCause(ActionCause cause)
		{
			base.SetCause(cause);
			this.Args.Cause = cause;
			return this;
		}
		public override BattleAction SetSource(GameEntity source)
		{
			base.SetSource(source);
			if (this.Args == null)
			{
				Debug.LogWarning("Args == null in " + base.GetType().Name);
			}
			else
			{
				this.Args.ActionSource = source;
			}
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
		public override GameEntity CancelSource
		{
			get
			{
				return this.Args.CancelSource;
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
