using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;
namespace LBoL.EntityLib.EnemyUnits.Opponent
{
	[UsedImplicitly]
	public sealed class Marisa : EnemyUnit
	{
		private Marisa.MoveType Next { get; set; }
		private Marisa.MoveType Last { get; set; }
		private string SpellCardName
		{
			get
			{
				return base.GetSpellCardName(new int?(5), 6);
			}
		}
		private int AddCardsCountDown { get; set; }
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Marisa.MoveType.ShootAddCard;
			base.CountDown = 5;
			this.AddCardsCountDown = 3;
			this._spellTimes = 0;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<FirepowerIsJustice>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction<FirepowerIsJustice>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}
		private IEnumerable<BattleAction> ChargeActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(4), true);
			yield return PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Marisa.MoveType.MultiShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + this._spellTimes, 3, false, "Instant", true);
				break;
			case Marisa.MoveType.ShootAddCard:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2 + this._spellTimes, 1, false, "Instant", true);
				yield return base.AddCardMove(null, typeof(Riguang), 1, EnemyUnit.AddCardZone.Discard, null, false);
				break;
			case Marisa.MoveType.ShootGraze:
				yield return base.AttackMove(base.GetMove(2), base.Gun3, base.Damage3 + this._spellTimes, 1, false, "Instant", true);
				yield return base.DefendMove(this, null, 0, 0, base.Defend, true, null);
				break;
			case Marisa.MoveType.AddCards:
			{
				List<Card> list = new List<Card>();
				list.Add(Library.CreateCard<Xingguang>());
				list.Add(Library.CreateCard<Xingguang>());
				list.Add(Library.CreateCard<Xingguang>());
				list.Add(Library.CreateCard<AyaNews>());
				List<Card> list2 = list;
				GameDifficulty difficulty = base.GameRun.Difficulty;
				if (difficulty != GameDifficulty.Hard)
				{
					if (difficulty == GameDifficulty.Lunatic)
					{
						list2.Add(Library.CreateCard<BlackResidue>());
					}
				}
				else
				{
					list2.Add(Library.CreateCard<HatateNews>());
				}
				list2.Shuffle(base.EnemyMoveRng);
				yield return base.AddCardMove(base.GetMove(3), list2, EnemyUnit.AddCardZone.Discard, PerformAction.Animation(this, "defend", 0f, null, 0f, -1), true);
				break;
			}
			case Marisa.MoveType.Charge:
				yield return new SimpleEnemyMove(Intention.Charge(), this.ChargeActions());
				break;
			case Marisa.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCardName, new int?(base.Damage4 + this._spellTimes * base.Count1), true), this.AttackActions(this.SpellCardName, base.Gun4, base.Damage4 + this._spellTimes * base.Count1, 1, true, "Instant"));
				this._spellTimes++;
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
				this.Next = Marisa.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(5, 6);
				return;
			}
			if (base.CountDown == 1 && base.Difficulty != GameDifficulty.Lunatic)
			{
				this.Next = Marisa.MoveType.Charge;
				return;
			}
			num = this.AddCardsCountDown - 1;
			this.AddCardsCountDown = num;
			if (this.AddCardsCountDown <= 0)
			{
				this.Next = Marisa.MoveType.AddCards;
				this.AddCardsCountDown = base.EnemyMoveRng.NextInt(4, 5);
				return;
			}
			this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
			this.Last = this.Next;
		}
		private int _spellTimes;
		private readonly RepeatableRandomPool<Marisa.MoveType> _pool = new RepeatableRandomPool<Marisa.MoveType>
		{
			{
				Marisa.MoveType.MultiShoot,
				1f
			},
			{
				Marisa.MoveType.ShootAddCard,
				1f
			},
			{
				Marisa.MoveType.ShootGraze,
				1f
			}
		};
		private enum MoveType
		{
			MultiShoot,
			ShootAddCard,
			ShootGraze,
			AddCards,
			Charge,
			Spell
		}
	}
}
