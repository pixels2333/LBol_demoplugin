using System;
using JetBrains.Annotations;
using LBoL.Core;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x0200010D RID: 269
	[UsedImplicitly]
	public sealed class AllCharacterCards : JadeBox
	{
		// Token: 0x060003B0 RID: 944 RVA: 0x0000A544 File Offset: 0x00008744
		protected override void OnAdded()
		{
			GameRunController gameRun = base.GameRun;
			int num = gameRun.AllCharacterCardsFlag + 1;
			gameRun.AllCharacterCardsFlag = num;
		}
	}
}
