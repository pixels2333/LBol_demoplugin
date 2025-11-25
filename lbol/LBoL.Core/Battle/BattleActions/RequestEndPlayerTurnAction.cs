using System;
namespace LBoL.Core.Battle.BattleActions
{
	public sealed class RequestEndPlayerTurnAction : SimpleAction
	{
		protected override void ResolvePhase()
		{
			base.Battle.PlayerTurnShouldEnd = true;
		}
	}
}
