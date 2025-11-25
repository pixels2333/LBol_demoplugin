using System;
using LBoL.Base.Extensions;
using LBoL.Core;
namespace LBoL.EntityLib.Exhibits.Common
{
	public sealed class Yuhangfu : Exhibit
	{
		protected override void OnEnterBattle()
		{
			base.HandleBattleEvent<DamageEventArgs>(base.Battle.Player.DamageTaking, new GameEventHandler<DamageEventArgs>(this.OnPlayerDamageTaking));
		}
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
