using System;
using System.Collections.Generic;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001AE RID: 430
	public class UnlockAllTurnManaAction : SimpleAction
	{
		// Token: 0x06000F59 RID: 3929 RVA: 0x000294B4 File Offset: 0x000276B4
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}

		// Token: 0x06000F5A RID: 3930 RVA: 0x000294DC File Offset: 0x000276DC
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			if (!base.Battle.LockedTurnMana.IsEmpty)
			{
				yield return new UnlockTurnManaAction(base.Battle.LockedTurnMana);
			}
			yield break;
		}

		// Token: 0x1700052E RID: 1326
		// (get) Token: 0x06000F5B RID: 3931 RVA: 0x000294EC File Offset: 0x000276EC
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
	}
}
