using System;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
namespace LBoL.EntityLib.Exhibits.Common
{
	[UsedImplicitly]
	public sealed class WanglingTideng : Exhibit
	{
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
		public override bool ShowCounter
		{
			get
			{
				return false;
			}
		}
		protected override void OnEnterBattle()
		{
			base.Active = true;
			base.HandleBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new GameEventHandler<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
		}
		protected override void OnLeaveBattle()
		{
			base.Active = false;
			base.Blackout = false;
		}
		private void OnBattleStarted(GameEventArgs args)
		{
			base.Counter = 0;
		}
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
