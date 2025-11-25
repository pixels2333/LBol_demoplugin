using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Adventure;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Normal.Drones
{
	public abstract class Drone : EnemyUnit
	{
		protected override void OnEnterBattle(BattleController battle)
		{
			this.EnterBattle();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DamageEventArgs>(base.DamageReceived, new Func<DamageEventArgs, IEnumerable<BattleAction>>(this.OnDamageReceived));
		}
		protected virtual void EnterBattle()
		{
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Appliance>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Appliance>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs arg)
		{
			if (arg.Cause == ActionCause.Card && arg.ActionSource is EmpCard)
			{
				int? num = new int?(2);
				yield return new ApplyStatusEffectAction<Emi>(this, default(int?), num, default(int?), default(int?), 0f, true);
				this.Stun();
			}
			yield break;
		}
		protected virtual void Stun()
		{
		}
	}
}
