using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
namespace LBoL.EntityLib.JadeBoxes
{
	[UsedImplicitly]
	public sealed class HpToPower : JadeBox
	{
		protected override void OnAdded()
		{
			base.HandleGameRunEvent<DamageEventArgs>(base.GameRun.Player.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnGamerunDamageReceived));
		}
		private void OnGamerunDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0f)
			{
				base.GameRun.GainPower((int)(args.DamageInfo.Damage * (float)base.Value1), false);
			}
		}
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<DamageEventArgs>(base.Battle.Player.DamageReceived, new EventSequencedReactor<DamageEventArgs>(this.OnBattleDamageReceived));
			base.ReactBattleEvent<UsUsingEventArgs>(base.Battle.UsUsed, new EventSequencedReactor<UsUsingEventArgs>(this.OnUsUsed));
		}
		private IEnumerable<BattleAction> OnBattleDamageReceived(DamageEventArgs args)
		{
			if (args.DamageInfo.Damage > 0f)
			{
				yield return new GainPowerAction((int)(args.DamageInfo.Damage * (float)base.Value1));
			}
			yield break;
		}
		private IEnumerable<BattleAction> OnUsUsed(UsUsingEventArgs args)
		{
			base.GameRun.LoseMaxHp(base.Value2, false);
			yield return null;
			yield break;
		}
	}
}
