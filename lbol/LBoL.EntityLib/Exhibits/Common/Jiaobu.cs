using System;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000175 RID: 373
	[UsedImplicitly]
	public sealed class Jiaobu : Exhibit
	{
		// Token: 0x06000532 RID: 1330 RVA: 0x0000CE67 File Offset: 0x0000B067
		protected override void OnGain(PlayerUnit player)
		{
			base.GameRun.UpgradeRandomCards(base.Value1, new CardType?(CardType.Defense));
		}

		// Token: 0x06000533 RID: 1331 RVA: 0x0000CE80 File Offset: 0x0000B080
		protected override void OnEnterBattle()
		{
			base.Blackout = true;
		}
	}
}
