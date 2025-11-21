using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x02000164 RID: 356
	[UsedImplicitly]
	public sealed class FaliJingdian : Exhibit
	{
		// Token: 0x060004E8 RID: 1256 RVA: 0x0000C7A4 File Offset: 0x0000A9A4
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UsUsingEventArgs>(base.Battle.UsUsed, new EventSequencedReactor<UsUsingEventArgs>(this.OnUsUsed));
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x0000C7C3 File Offset: 0x0000A9C3
		private IEnumerable<BattleAction> OnUsUsed(UsUsingEventArgs args)
		{
			base.NotifyActivating();
			yield return new GainManaAction(base.Mana);
			yield break;
		}
	}
}
