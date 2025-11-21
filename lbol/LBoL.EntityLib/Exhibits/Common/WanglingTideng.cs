using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001A3 RID: 419
	[UsedImplicitly]
	public sealed class WanglingTideng : Exhibit
	{
		// Token: 0x1700007F RID: 127
		// (get) Token: 0x060005FD RID: 1533 RVA: 0x0000E0EE File Offset: 0x0000C2EE
		public override string OverrideIconName
		{
			get
			{
				if (base.Counter != 0)
				{
					return base.Id + "Inactive";
				}
				return base.Id;
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x060005FE RID: 1534 RVA: 0x0000E10F File Offset: 0x0000C30F
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060005FF RID: 1535 RVA: 0x0000E114 File Offset: 0x0000C314
		protected override void OnEnterBattle()
		{
			base.Active = true;
			base.HandleBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new GameEventHandler<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
		}

		// Token: 0x06000600 RID: 1536 RVA: 0x0000E167 File Offset: 0x0000C367
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}

		// Token: 0x06000601 RID: 1537 RVA: 0x0000E177 File Offset: 0x0000C377
		private void OnBattleStarted(GameEventArgs args)
		{
			base.Counter = 0;
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x0000E180 File Offset: 0x0000C380
		private void OnPlayerDamageTaking(DamageEventArgs args)
		{
			if (base.Counter == 0)
			{
				DamageInfo damageInfo = args.DamageInfo;
				int num = damageInfo.Damage.RoundToInt();
				if (num >= 1)
				{
					base.NotifyActivating();
					base.Counter = 1;
					args.DamageInfo = damageInfo.ReduceActualDamageBy(num);
					args.AddModifier(this);
					base.Active = false;
					base.Blackout = true;
				}
			}
		}
	}
}
