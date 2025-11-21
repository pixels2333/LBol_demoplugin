using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Mixins;

namespace LBoL.EntityLib.StatusEffects.Enemy
{
	// Token: 0x020000B6 RID: 182
	[UsedImplicitly]
	public sealed class LouguanJianSe : StatusEffect, ILouguanJian, INotifyActivating
	{
		// Token: 0x06000275 RID: 629 RVA: 0x00006F78 File Offset: 0x00005178
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
		}

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000276 RID: 630 RVA: 0x00006F98 File Offset: 0x00005198
		public Unit LouguanJianOwner
		{
			get
			{
				return base.Owner;
			}
		}

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000277 RID: 631 RVA: 0x00006FA0 File Offset: 0x000051A0
		int ILouguanJian.Multiplier
		{
			get
			{
				return base.Level;
			}
		}

		// Token: 0x06000278 RID: 632 RVA: 0x00006FA8 File Offset: 0x000051A8
		void ILouguanJian.OnTriggered()
		{
			int num = this.TriggerCount + 1;
			this.TriggerCount = num;
		}

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x06000279 RID: 633 RVA: 0x00006FC5 File Offset: 0x000051C5
		// (set) Token: 0x0600027A RID: 634 RVA: 0x00006FCD File Offset: 0x000051CD
		public int TriggerCount { get; private set; }

		// Token: 0x0600027C RID: 636 RVA: 0x00006FDE File Offset: 0x000051DE
		BattleController ILouguanJian.get_Battle()
		{
			return base.Battle;
		}
	}
}
