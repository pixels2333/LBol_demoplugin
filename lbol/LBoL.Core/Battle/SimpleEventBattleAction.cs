using System;
using System.Collections.Generic;
namespace LBoL.Core.Battle
{
	public abstract class SimpleEventBattleAction<TArgs> : EventBattleAction<TArgs> where TArgs : GameEventArgs
	{
		internal override IEnumerable<Phase> GetPhases()
		{
			yield return base.CreatePhase("PreEvent", new Action(this.PreEventPhase), false);
			yield return base.CreatePhase("Main", delegate
			{
				this.MainPhase();
				base.Args.CanCancel = false;
			}, true);
			yield return base.CreatePhase("PostEvent", new Action(this.PostEventPhase), false);
			yield break;
		}
		protected virtual void PreEventPhase()
		{
		}
		protected virtual void MainPhase()
		{
		}
		protected virtual void PostEventPhase()
		{
		}
		internal void Trigger(GameEvent<TArgs> e)
		{
			e.Execute(base.Args);
		}
	}
}
