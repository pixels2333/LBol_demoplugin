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
	[UsedImplicitly]
	public sealed class Doremy : EnemyUnit<IDoremyView>
	{
		private Doremy.MoveType Next { get; set; }
		private int DoremyLevel { get; set; }
		private int MoveCountDown { get; set; }
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
		public IEnumerable<BattleAction> OnNightmareHappened()
		{
			yield return PerformAction.Chat(this, "Chat.DoremyKill".Localize(true), 3f, 0f, 2f, true);
			yield break;
		}
		private StatusEffect Sleep { get; set; }
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
		private void OnBattleEnding(GameEventArgs arg)
		{
			IDoremyView view = base.View;
			if (view == null)
			{
				return;
			}
			view.SetEffect(false, 0);
		}
		private IEnumerable<BattleAction> OnDamageReceived(DamageEventArgs arg)
		{
			if (base.HasStatusEffect<Sleep>() && arg.DamageInfo.Damage > 0f && !(arg.ActionSource is HeiseBijiben) && !(arg.ActionSource is QuickAct1))
			{
				return this.AwakeActions(true, !base.IsInTurn);
			}
			return null;
		}
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
		private int DreamTurn { get; set; }
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
		public bool IsWokeUp { get; private set; }
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
		private const int AddHp = 50;
		private readonly List<Type> _servants;
		private enum MoveType
		{
			Sleep,
			Stun,
			Shoot,
			Defend
		}
	}
}
