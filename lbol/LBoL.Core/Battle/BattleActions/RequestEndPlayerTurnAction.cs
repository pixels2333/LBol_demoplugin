using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x0200019E RID: 414
	public sealed class RequestEndPlayerTurnAction : SimpleAction
	{
		// Token: 0x06000F16 RID: 3862 RVA: 0x00028BBC File Offset: 0x00026DBC
		protected override void ResolvePhase()
		{
			base.Battle.PlayerTurnShouldEnd = true;
		}
	}
}
