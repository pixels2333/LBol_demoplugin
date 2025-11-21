using System;
using LBoL.Base.Extensions;
using LBoL.Core;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x020001B0 RID: 432
	public sealed class Yuhangfu : Exhibit
	{
		// Token: 0x06000634 RID: 1588 RVA: 0x0000E64B File Offset: 0x0000C84B
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x0000E670 File Offset: 0x0000C870
		private void OnPlayerDamageTaking(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage.RoundToInt() > 0)
			{
				base.NotifyActivating();
				args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(base.Value1);
				args.AddModifier(this);
			}
		}
	}
}
