using System;
using System.Collections.Generic;
namespace LBoL.Core.Battle
{
	public abstract class SimpleAction : BattleAction
	{
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("Resolve", new Action(this.ResolvePhase), true);
			yield break;
		}
		protected virtual void ResolvePhase()
		{
		}
		public override bool IsModified
		{
			get
			{
				return false;
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
		}
		public override string ExportDebugDetails()
		{
			return null;
		}
	}
}
