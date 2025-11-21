using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001AA RID: 426
	[UsedImplicitly]
	public sealed class Yaoshi : Exhibit
	{
		// Token: 0x0600061E RID: 1566 RVA: 0x0000E41E File Offset: 0x0000C61E
		protected override void OnEnterBattle()
		{
			base.Active = true;
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnPlayerDamageReceived));
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x0000E449 File Offset: 0x0000C649
		private IEnumerable<BattleAction> OnPlayerDamageReceived(DamageEventArgs args)
		{
			if (base.Active && args.DamageInfo.Damage > 0f)
			{
				base.NotifyActivating();
				base.Active = false;
				yield return new GainManaAction(base.Mana);
				base.Blackout = true;
			}
			yield break;
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x0000E460 File Offset: 0x0000C660
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
	}
}
