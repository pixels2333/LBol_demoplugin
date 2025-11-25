using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	[UsedImplicitly]
	public sealed class Sakuya : EnemyUnit
	{
		private Sakuya.MoveType Next { get; set; }
		private Sakuya.MoveType Last { get; set; }
		private string SpellCardName
		{
			get
			{
				return base.GetSpellCardName(new int?(4), 5);
			}
		}
		private int AddCardsCountDown { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Sakuya.MoveType.MultiShoot;
			base.CountDown = 5;
			this.AddCardsCountDown = 1;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?((base.GameRun.Difficulty == GameDifficulty.Lunatic) ? 10 : 5);
			int? num2 = new int?(8);
			yield return new ApplyStatusEffectAction<PrivateSquare>(this, num, default(int?), default(int?), num2, 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			int? num = new int?((base.GameRun.Difficulty == GameDifficulty.Lunatic) ? 10 : 5);
			int? num2 = new int?(8);
			this.React(new ApplyStatusEffectAction<PrivateSquare>(this, num, default(int?), default(int?), num2, 0f, true));
			num2 = default(int?);
			int? num3 = num2;
			num2 = default(int?);
			int? num4 = num2;
			num2 = default(int?);
			int? num5 = num2;
			num2 = default(int?);
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, num3, num4, num5, num2, 0f, true));
		}
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun4, base.Damage4, 1, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			int? num = new int?(5);
			int? num2 = new int?(8);
			yield return new ApplyStatusEffectAction<PrivateSquare>(this, num, default(int?), default(int?), num2, 0f, true);
			yield break;
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Sakuya.MoveType.MultiShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 4, false, "Instant", true);
				break;
			case Sakuya.MoveType.ShootDebuff:
			{
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", true);
				string text = null;
				Type typeFromHandle = typeof(Fragil);
				int? num = new int?(1);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, false, false, null);
				break;
			}
			case Sakuya.MoveType.Defend:
				yield return base.DefendMove(this, base.GetMove(2), 0, base.Defend, base.Count1, true, null);
				break;
			case Sakuya.MoveType.AddCards:
				yield return base.AddCardMove(base.GetMove(3), typeof(SakuyaLock), 2, EnemyUnit.AddCardZone.Draw, PerformAction.Animation(this, "shoot3", 0.3f, null, 0f, -1), true);
				break;
			case Sakuya.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCardName, new int?(base.Damage4), true), this.SpellActions());
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
			if (base.CountDown <= 0)
			{
				this.Next = Sakuya.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(5, 6);
				return;
			}
			num = this.AddCardsCountDown - 1;
			this.AddCardsCountDown = num;
			if (this.AddCardsCountDown <= 0)
			{
				this.Next = Sakuya.MoveType.AddCards;
				this.AddCardsCountDown = base.EnemyMoveRng.NextInt(3, 4);
				return;
			}
			this.Last = this.Next;
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
		}
		private readonly RepeatableRandomPool<Sakuya.MoveType> _pool = new RepeatableRandomPool<Sakuya.MoveType>
		{
			{
				Sakuya.MoveType.MultiShoot,
				2f
			},
			{
				Sakuya.MoveType.ShootDebuff,
				2f
			},
			{
				Sakuya.MoveType.Defend,
				1f
			}
		};
		private enum MoveType
		{
			MultiShoot,
			ShootDebuff,
			Defend,
			AddCards,
			Spell
		}
	}
}
