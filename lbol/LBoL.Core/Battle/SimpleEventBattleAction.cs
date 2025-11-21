using System;
using System.Collections.Generic;

namespace LBoL.Core.Battle
{
	// Token: 0x0200014E RID: 334
	public abstract class SimpleEventBattleAction<TArgs> : EventBattleAction<TArgs> where TArgs : GameEventArgs
	{
		// Token: 0x06000D6F RID: 3439 RVA: 0x0002563E File Offset: 0x0002383E
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

		// Token: 0x06000D70 RID: 3440 RVA: 0x0002564E File Offset: 0x0002384E
		protected virtual void PreEventPhase()
		{
		}

		// Token: 0x06000D71 RID: 3441 RVA: 0x00025650 File Offset: 0x00023850
		protected virtual void MainPhase()
		{
		}

		// Token: 0x06000D72 RID: 3442 RVA: 0x00025652 File Offset: 0x00023852
		protected virtual void PostEventPhase()
		{
		}

		// Token: 0x06000D73 RID: 3443 RVA: 0x00025654 File Offset: 0x00023854
		internal void Trigger(GameEvent<TArgs> e)
		{
			e.Execute(base.Args);
		}
	}
}
