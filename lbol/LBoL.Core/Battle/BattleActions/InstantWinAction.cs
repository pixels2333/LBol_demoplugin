using System;
namespace LBoL.Core.Battle.BattleActions
{
	public class InstantWinAction : SimpleAction
	{
		protected override void ResolvePhase()
		{
			base.Battle.InstantWin();
		}
	}
}
