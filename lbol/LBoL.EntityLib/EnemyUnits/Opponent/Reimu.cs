using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Others;
namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	[UsedImplicitly]
	public sealed class Reimu : EnemyUnit
	{
		private string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(5), 6);
			}
		}
		private Reimu.MoveType LastAttack { get; set; }
		private Reimu.MoveType Next { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.LastAttack = Reimu.MoveType.ShootAccuracy;
			this.Next = Reimu.MoveType.Summon;
			base.CountDown = 5;
			this._defendCount = 2;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new CastBlockShieldAction(this, 0, base.Defend, BlockShieldType.Normal, false);
			yield return new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new CastBlockShieldAction(this, 0, base.Defend, BlockShieldType.Normal, false));
			this.React(new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}
		private IEnumerable<BattleAction> HakureiDefend()
		{
			yield return new EnemyMoveAction(this, base.GetMove(3), true);
			yield return new CastBlockShieldAction(this, 0, base.Defend + base.EnemyBattleRng.NextInt(0, 2), BlockShieldType.Normal, true);
			if (!base.HasStatusEffect<Amulet>())
			{
				yield return new ApplyStatusEffectAction<Amulet>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			yield break;
		}
		private IEnumerable<BattleAction> SummonActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(4), true);
			yield return PerformAction.Animation(this, "shoot2", 0.5f, null, 0f, -1);
			if (Enumerable.All<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex != 0))
			{
				yield return new SpawnEnemyAction<YinyangyuRedReimu>(this, 0, 0f, 0.3f, true);
			}
			if (Enumerable.All<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex != 1))
			{
				yield return new SpawnEnemyAction<YinyangyuBlueReimu>(this, 1, 0f, 0.3f, true);
			}
			this._vacancy = 7;
			yield break;
		}
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(this.SpellCard, base.Gun4, base.Damage4, 3, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			yield break;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Reimu.MoveType.MultiShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 4, false, "Instant", true);
				break;
			case Reimu.MoveType.ShootAccuracy:
			{
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, true, "Instant", true);
				string text = null;
				Type typeFromHandle = typeof(Weak);
				int? num = new int?(1);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, false, false, null);
				break;
			}
			case Reimu.MoveType.ShootDebuff:
			{
				yield return base.AttackMove(base.GetMove(2), base.Gun3, base.Damage3, 1, false, "Instant", true);
				string text2 = null;
				Type typeFromHandle2 = typeof(Fengyin);
				int? num = new int?(1);
				yield return base.NegativeMove(text2, typeFromHandle2, default(int?), num, false, false, null);
				break;
			}
			case Reimu.MoveType.Defend:
				yield return new SimpleEnemyMove(Intention.Defend().WithMoveName(base.GetMove(3)), this.HakureiDefend());
				break;
			case Reimu.MoveType.Summon:
				yield return new SimpleEnemyMove(Intention.Spawn().WithMoveName(base.GetMove(4)), this.SummonActions());
				break;
			case Reimu.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCard, new int?(base.Damage4), new int?(3), true), this.SpellActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			int countDown = base.CountDown;
			if (countDown == 1 || countDown == 2)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			int num2 = Enumerable.Count<EnemyUnit>(base.Battle.AllAliveEnemies);
			int num3 = 3 - num2;
			this._vacancy -= num3;
			if (base.CountDown <= 0)
			{
				this.Next = Reimu.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(5, 6);
				return;
			}
			if (this._vacancy <= 0)
			{
				this.Next = Reimu.MoveType.Summon;
				return;
			}
			this._defendCount--;
			if (this._defendCount <= 0 && base.Shield == 0)
			{
				this.Next = Reimu.MoveType.Defend;
				this._defendCount = 4;
				return;
			}
			this.Next = this._pool.Without(this.LastAttack).Sample(base.EnemyMoveRng);
			this.LastAttack = this.Next;
		}
		private readonly RepeatableRandomPool<Reimu.MoveType> _pool = new RepeatableRandomPool<Reimu.MoveType>
		{
			{
				Reimu.MoveType.MultiShoot,
				2f
			},
			{
				Reimu.MoveType.ShootAccuracy,
				2f
			},
			{
				Reimu.MoveType.ShootDebuff,
				1f
			}
		};
		private int _defendCount;
		private int _vacancy;
		private enum MoveType
		{
			MultiShoot,
			ShootAccuracy,
			ShootDebuff,
			Defend,
			Summon,
			Spell
		}
	}
}
