using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B6 RID: 438
	[UsedImplicitly]
	public sealed class Zhuzi : Exhibit
	{
		// Token: 0x0600064E RID: 1614 RVA: 0x0000E9B8 File Offset: 0x0000CBB8
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived));
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x0000E9DC File Offset: 0x0000CBDC
		private IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.IsGrazed)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
