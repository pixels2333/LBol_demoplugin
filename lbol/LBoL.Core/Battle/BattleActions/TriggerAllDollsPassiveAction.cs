using System;
using System.Collections.Generic;
using LBoL.Core.Units;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x020001AB RID: 427
	public sealed class TriggerAllDollsPassiveAction : SimpleAction
	{
		// Token: 0x06000F52 RID: 3922 RVA: 0x00029410 File Offset: 0x00027610
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x00029438 File Offset: 0x00027638
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			IReadOnlyList<Doll> dolls = base.Battle.Player.Dolls;
			foreach (Doll doll in dolls)
			{
				yield return new TriggerDollPassiveAction(doll, false);
			}
			IEnumerator<Doll> enumerator = null;
			yield break;
			yield break;
		}
	}
}
