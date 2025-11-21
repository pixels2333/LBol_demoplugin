using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.JadeBoxes
{
	// Token: 0x02000114 RID: 276
	[UsedImplicitly]
	public sealed class HpToPower : JadeBox
	{
		// Token: 0x060003CB RID: 971 RVA: 0x0000A932 File Offset: 0x00008B32
		protected override void OnAdded()
		{
			base.HandleGameRunEvent<DamageEventArgs>(base.GameRun.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnGamerunDamageReceived));
		}

		// Token: 0x060003CC RID: 972 RVA: 0x0000A958 File Offset: 0x00008B58
		private void OnGamerunDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0f)
			{
				base.GameRun.GainPower((int)(args.DamageInfo.Damage * (float)base.Value1), false);
			}
		}

		// Token: 0x060003CD RID: 973 RVA: 0x0000A9A0 File Offset: 0x00008BA0
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnBattleDamageReceived));
			base.ReactBattleEvent<UsUsingEventArgs>(base.Battle.UsUsed, new EventSequencedReactor<UsUsingEventArgs>(this.OnUsUsed));
		}

		// Token: 0x060003CE RID: 974 RVA: 0x0000A9EC File Offset: 0x00008BEC
		private IEnumerable<BattleAction> OnBattleDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0f)
			{
				yield return new GainPowerAction((int)(args.DamageInfo.Damage * (float)base.Value1));
			}
			yield break;
		}

		// Token: 0x060003CF RID: 975 RVA: 0x0000AA03 File Offset: 0x00008C03
		private IEnumerable<BattleAction> OnUsUsed(UsUsingEventArgs args)
		{
			base.GameRun.LoseMaxHp(base.Value2, false);
			yield return null;
			yield break;
		}
	}
}
