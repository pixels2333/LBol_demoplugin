using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Character.DreamServants;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.JadeBoxes;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000239 RID: 569
	[UsedImplicitly]
	public sealed class Doremy : EnemyUnit<IDoremyView>
	{
		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000898 RID: 2200 RVA: 0x00012B4F File Offset: 0x00010D4F
		// (set) Token: 0x06000899 RID: 2201 RVA: 0x00012B57 File Offset: 0x00010D57
		private Doremy.MoveType Next { get; set; }

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x0600089A RID: 2202 RVA: 0x00012B60 File Offset: 0x00010D60
		// (set) Token: 0x0600089B RID: 2203 RVA: 0x00012B68 File Offset: 0x00010D68
		private int DoremyLevel { get; set; }

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x0600089C RID: 2204 RVA: 0x00012B71 File Offset: 0x00010D71
		// (set) Token: 0x0600089D RID: 2205 RVA: 0x00012B79 File Offset: 0x00010D79
		private int MoveCountDown { get; set; }

		// Token: 0x0600089E RID: 2206 RVA: 0x00012B84 File Offset: 0x00010D84
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Doremy.MoveType.Sleep;
			this.MoveCountDown = 2;
			this.DreamTurn = 0;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.HandleBattleEvent<GameEventArgs>(base.Battle.BattleEnding, new GameEventHandler<GameEventArgs>(this.OnBattleEnding));
			base.ReactBattleEvent<DamageEventArgs>(base.DamageReceived, new Func<DamageEventArgs, IEnumerable<BattleAction>>(this.OnDamageReceived));
			base.ReactBattleEvent<DieEventArgs>(base.Died, new Func<DieEventArgs, IEnumerable<BattleAction>>(this.OnDied));
			this.DoremyLevel = 0;
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x00012C17 File Offset: 0x00010E17
		public IEnumerable<BattleAction> OnNightmareHappened()
		{
			yield return PerformAction.Chat(this, "Chat.DoremyKill".Localize(true), 3f, 0f, 2f, true);
			yield break;
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060008A0 RID: 2208 RVA: 0x00012C27 File Offset: 0x00010E27
		// (set) Token: 0x060008A1 RID: 2209 RVA: 0x00012C2F File Offset: 0x00010E2F
		private StatusEffect Sleep { get; set; }

		// Token: 0x060008A2 RID: 2210 RVA: 0x00012C38 File Offset: 0x00010E38
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			IDoremyView view = base.View;
			if (view != null)
			{
				view.SetEffect(true, 0);
			}
			int? num = new int?(base.Count2);
			int? num2 = new int?(3);
			ApplyStatusEffectAction<Sleep> buff = new ApplyStatusEffectAction<Sleep>(this, num, default(int?), num2, default(int?), 0f, true);
			yield return buff;
			this.Sleep = buff.Args.Effect;
			yield break;
		}

		// Token: 0x060008A3 RID: 2211 RVA: 0x00012C48 File Offset: 0x00010E48
		private void OnBattleEnding(GameEventArgs arg)
		{
			IDoremyView view = base.View;
			if (view == null)
			{
				return;
			}
			view.SetEffect(false, 0);
		}

		// Token: 0x060008A4 RID: 2212 RVA: 0x00012C5C File Offset: 0x00010E5C
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs arg)
		{
			if (base.HasStatusEffect<Sleep>() && arg.DamageInfo.Damage > 0f && !(arg.ActionSource is HeiseBijiben) && !(arg.ActionSource is QuickAct1))
			{
				return this.AwakeActions(true, !base.IsInTurn);
			}
			return null;
		}

		// Token: 0x060008A5 RID: 2213 RVA: 0x00012CB2 File Offset: 0x00010EB2
		private IEnumerable<BattleAction> OnDied(DieEventArgs arg)
		{
			if (Enumerable.Any<EnemyUnit>(base.AllAliveEnemies))
			{
				foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
				{
					yield return new EscapeAction(enemyUnit);
				}
				IEnumerator<EnemyUnit> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x060008A6 RID: 2214 RVA: 0x00012CC2 File Offset: 0x00010EC2
		private IEnumerable<BattleAction> SleepActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(0), true);
			IDoremyView view = base.View;
			if (view != null)
			{
				bool flag = true;
				int num = this.DoremyLevel + 1;
				this.DoremyLevel = num;
				view.SetEffect(flag, num);
			}
			base.GameRun.SetEnemyHpAndMaxHp(base.Hp + 50, base.MaxHp + 50, this, false);
			yield return PerformAction.Effect(this, "DreamDeeper", 0f, "Sleep", 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return new ApplyStatusEffectAction<DoremyLevel>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			if (this.Sleep.Count > 1)
			{
				StatusEffect sleep = this.Sleep;
				int num = sleep.Count - 1;
				sleep.Count = num;
				if (this.Sleep.Count == 1)
				{
					yield return new ApplyStatusEffectAction<Sleep>(this, new int?(base.Count2), default(int?), default(int?), default(int?), 0f, true);
					yield return PerformAction.Chat(this, "Chat.DoremySleepLast".Localize(true), 3f, 0f, 0f, false);
				}
				else
				{
					yield return PerformAction.Chat(this, "Chat.DoremySleep".Localize(true), 2f, 0f, 0f, false);
				}
			}
			else
			{
				foreach (BattleAction battleAction in this.AwakeActions(false, false))
				{
					yield return battleAction;
				}
				IEnumerator<BattleAction> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x060008A7 RID: 2215 RVA: 0x00012CD2 File Offset: 0x00010ED2
		private IEnumerable<BattleAction> AwakeActions(bool stun = false, bool updateTurnMove = false)
		{
			IDoremyView view = base.View;
			if (view != null)
			{
				view.SetSleep(false);
			}
			Sleep statusEffect = base.GetStatusEffect<Sleep>();
			yield return new RemoveStatusEffectAction(statusEffect, true, 0.1f);
			if (base.Shield > 0)
			{
				yield return new LoseBlockShieldAction(this, 0, base.Shield, false);
			}
			this.Next = (stun ? Doremy.MoveType.Stun : Doremy.MoveType.Shoot);
			if (stun)
			{
				yield return PerformAction.Chat(this, "Chat.DoremyAwake".Localize(true), 3f, 0f, 0.5f, true);
			}
			else
			{
				this.IsWokeUp = true;
			}
			if (this.DoremyLevel >= 3)
			{
				yield return new ApplyStatusEffectAction<DreamMaster>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
				base.CountDown = 0;
			}
			else
			{
				base.CountDown = 6 - this.DoremyLevel;
			}
			if (updateTurnMove)
			{
				base.UpdateTurnMoves();
			}
			yield break;
		}

		// Token: 0x060008A8 RID: 2216 RVA: 0x00012CF0 File Offset: 0x00010EF0
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Doremy.MoveType.Sleep:
				yield return new SimpleEnemyMove(Intention.Sleep(), this.SleepActions());
				break;
			case Doremy.MoveType.Stun:
				yield return new SimpleEnemyMove(Intention.Stun(), new EnemyMoveAction[]
				{
					new EnemyMoveAction(this, base.GetMove(1), true)
				});
				this.Next = Doremy.MoveType.Shoot;
				break;
			case Doremy.MoveType.Shoot:
			{
				if (this.MoveCountDown == 2)
				{
					yield return base.AttackMove(base.GetMove(2), base.Gun1, (this.DoremyLevel >= 2) ? (base.Damage1 + base.Damage2) : base.Damage1, 4, false, "Instant", false);
				}
				else
				{
					yield return base.AttackMove(base.GetMove(3), base.Gun2, (this.DoremyLevel >= 2) ? (base.Damage3 + base.Damage4) : base.Damage3, 1, true, "Instant", false);
				}
				if (base.HasStatusEffect<DreamMaster>())
				{
					yield return new SimpleEnemyMove(Intention.Spawn(), this.DreamMasterActions());
				}
				if (this.DoremyLevel >= 1)
				{
					yield return base.AddCardMove(null, Library.CreateCards<Nightmare>(1, false), EnemyUnit.AddCardZone.Hand, null, false);
				}
				int num = this.MoveCountDown - 1;
				this.MoveCountDown = num;
				if (this.MoveCountDown <= 0)
				{
					this.Next = Doremy.MoveType.Defend;
				}
				break;
			}
			case Doremy.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(3), 0, (this.DoremyLevel >= 2) ? (base.Defend + base.Count1) : base.Defend, 0, true, null);
				if (base.HasStatusEffect<DreamMaster>())
				{
					yield return new SimpleEnemyMove(Intention.Spawn(), this.DreamMasterActions());
				}
				yield return base.AddCardMove(null, Library.CreateCards<Nightmare>(1, false), EnemyUnit.AddCardZone.Hand, null, false);
				this.Next = Doremy.MoveType.Shoot;
				this.MoveCountDown = 2;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (base.CountDown > 0)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown), this.CountDownActions());
			}
			yield break;
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060008A9 RID: 2217 RVA: 0x00012D00 File Offset: 0x00010F00
		// (set) Token: 0x060008AA RID: 2218 RVA: 0x00012D08 File Offset: 0x00010F08
		private int DreamTurn { get; set; }

		// Token: 0x060008AB RID: 2219 RVA: 0x00012D11 File Offset: 0x00010F11
		private IEnumerable<BattleAction> DreamMasterActions()
		{
			if (this.DreamTurn % this._servants.Count == 0)
			{
				this._servants.Shuffle(base.EnemyBattleRng);
			}
			yield return PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1);
			yield return new SpawnEnemyAction(this, this._servants[this.DreamTurn % this._servants.Count], this.DreamTurn % 2, 0f, 0.3f, false);
			int num = this.DreamTurn + 1;
			this.DreamTurn = num;
			yield break;
		}

		// Token: 0x060008AC RID: 2220 RVA: 0x00012D21 File Offset: 0x00010F21
		private IEnumerable<BattleAction> CountDownActions()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				yield return new ApplyStatusEffectAction<DreamMaster>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060008AD RID: 2221 RVA: 0x00012D31 File Offset: 0x00010F31
		// (set) Token: 0x060008AE RID: 2222 RVA: 0x00012D39 File Offset: 0x00010F39
		public bool IsWokeUp { get; private set; }

		// Token: 0x060008AF RID: 2223 RVA: 0x00012D44 File Offset: 0x00010F44
		public Doremy()
		{
			List<Type> list = new List<Type>();
			list.Add(typeof(DreamRemilia));
			list.Add(typeof(DreamAya));
			list.Add(typeof(DreamJunko));
			list.Add(typeof(DreamYoumu));
			this._servants = list;
			base..ctor();
		}

		// Token: 0x040000A2 RID: 162
		private const int AddHp = 50;

		// Token: 0x040000A3 RID: 163
		private readonly List<Type> _servants;

		// Token: 0x02000724 RID: 1828
		private enum MoveType
		{
			// Token: 0x04000A1F RID: 2591
			Sleep,
			// Token: 0x04000A20 RID: 2592
			Stun,
			// Token: 0x04000A21 RID: 2593
			Shoot,
			// Token: 0x04000A22 RID: 2594
			Defend
		}
	}
}
