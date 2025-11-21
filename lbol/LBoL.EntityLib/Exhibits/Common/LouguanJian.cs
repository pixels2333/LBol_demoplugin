using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using LBoL.EntityLib.Mixins;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200017C RID: 380
	[UsedImplicitly]
	public sealed class LouguanJian : Exhibit, ILouguanJian, INotifyActivating
	{
		// Token: 0x06000552 RID: 1362 RVA: 0x0000D169 File Offset: 0x0000B369
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<StatisticalDamageEventArgs>(base.Battle.Player.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000553 RID: 1363 RVA: 0x0000D18E File Offset: 0x0000B38E
		public Unit LouguanJianOwner
		{
			get
			{
				return base.Owner;
			}
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000554 RID: 1364 RVA: 0x0000D196 File Offset: 0x0000B396
		int ILouguanJian.Multiplier
		{
			get
			{
				return base.Value1;
			}
		}

		// Token: 0x06000556 RID: 1366 RVA: 0x0000D1A6 File Offset: 0x0000B3A6
		BattleController ILouguanJian.get_Battle()
		{
			return base.Battle;
		}
	}
}
