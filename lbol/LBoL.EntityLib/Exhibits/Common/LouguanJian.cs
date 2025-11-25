using System;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using LBoL.EntityLib.Mixins;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class LouguanJian : Exhibit, ILouguanJian, INotifyActivating
	{
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<StatisticalDamageEventArgs>(base.Battle.Player.StatisticalTotalDamageDealt, new EventSequencedReactor<StatisticalDamageEventArgs>(this.OnStatisticalDamageDealt));
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
				return base.Value1;
			}
		}
		BattleController ILouguanJian.get_Battle()
		{
			return base.Battle;
		}
	}
}
