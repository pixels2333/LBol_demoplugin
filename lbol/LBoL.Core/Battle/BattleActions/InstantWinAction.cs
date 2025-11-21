using System;

namespace LBoL.Core.Battle.BattleActions
{
	// Token: 0x02000189 RID: 393
	public class InstantWinAction : SimpleAction
	{
		// Token: 0x06000EAD RID: 3757 RVA: 0x00027DE7 File Offset: 0x00025FE7
		protected override void ResolvePhase()
		{
			base.Battle.InstantWin();
		}
	}
}
