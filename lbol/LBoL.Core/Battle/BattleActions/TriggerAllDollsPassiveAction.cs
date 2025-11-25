using System;
using System.Collections.Generic;
using LBoL.Core.Units;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class TriggerAllDollsPassiveAction : SimpleAction
	{
		protected override void ResolvePhase()
		{
			base.React(new Reactor(this.ResolvePhaseEnumerator()), null, default(ActionCause?));
		}
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
