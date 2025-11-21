using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200015C RID: 348
	[UsedImplicitly]
	public sealed class Chaidao : Exhibit
	{
		// Token: 0x060004C9 RID: 1225 RVA: 0x0000C49B File Offset: 0x0000A69B
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.UpgradeRandomCards(base.Value1, new CardType?(CardType.Attack));
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x0000C4B4 File Offset: 0x0000A6B4
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
