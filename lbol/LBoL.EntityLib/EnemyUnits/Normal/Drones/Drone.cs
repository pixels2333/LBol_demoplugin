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
	// Token: 0x02000201 RID: 513
	public abstract class Drone : EnemyUnit
	{
		// Token: 0x06000815 RID: 2069 RVA: 0x00011F30 File Offset: 0x00010130
		protected override void OnEnterBattle(BattleController battle)
		{
			this.EnterBattle();
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<DamageEventArgs>(base.DamageReceived, new Func<DamageEventArgs, IEnumerable<BattleAction>>(this.OnDamageReceived));
		}

		// Token: 0x06000816 RID: 2070 RVA: 0x00011F6D File Offset: 0x0001016D
		protected virtual void EnterBattle()
		{
		}

		// Token: 0x06000817 RID: 2071 RVA: 0x00011F6F File Offset: 0x0001016F
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Appliance>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x06000818 RID: 2072 RVA: 0x00011F80 File Offset: 0x00010180
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<Appliance>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x06000819 RID: 2073 RVA: 0x00011FC8 File Offset: 0x000101C8
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

		// Token: 0x0600081A RID: 2074 RVA: 0x00011FDF File Offset: 0x000101DF
		protected virtual void Stun()
		{
		}
	}
}
