using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Mixins;
namespace LBoL.EntityLib.StatusEffects.Enemy
{
	[UsedImplicitly]
	public sealed class LouguanJianSe : StatusEffect, ILouguanJian, INotifyActivating
	{
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<StatisticalDamageEventArgs>(base.Owner.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
		}
		public Unit LouguanJianOwner
		{
			get
			{
				return base.Owner;
			}
		}
		int ILouguanJian.Multiplier
		{
			get
			{
				return base.Level;
			}
		}
		void ILouguanJian.OnTriggered()
		{
			int num = this.TriggerCount + 1;
			this.TriggerCount = num;
		}
		public int TriggerCount { get; private set; }
		BattleController ILouguanJian.get_Battle()
		{
			return base.Battle;
		}
	}
}
