using System;
using System.Collections.Generic;
namespace LBoL.Core.Battle.BattleActions
{
	public class UnlockAllTurnManaAction : SimpleAction
	{
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}
		private IEnumerable<BattleAction> ResolvePhaseEnumerator()
		{
			if (!base.Battle.LockedTurnMana.IsEmpty)
			{
				yield return new UnlockTurnManaAction(base.Battle.LockedTurnMana);
			}
			yield break;
		}
		public override bool IsCanceled
		{
			get
			{
				return false;
			}
		}
	}
}
