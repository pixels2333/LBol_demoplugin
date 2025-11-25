using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
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
	public sealed class Koishi : EnemyUnit
	{
		private Koishi.MoveType Next { get; set; }
		private Koishi.MoveType Last { get; set; }
		private Koishi.InspirationType PreviousInspiration { get; set; }
		private Koishi.InspirationType NextInspiration { get; set; }
		private RandomGen InspirationRng { get; set; }
		private string SpellCardName
		{
			get
			{
				return base.GetSpellCardName(new int?(9), 10);
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			base.CountDown = 3;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			this.InspirationRng = base.EnemyMoveRng;
			this.NextInspiration = this._insPool.Sample(this.InspirationRng);
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				yield return new ApplyStatusEffectAction<KoishiUnknown>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
				this.SpellLevel = 1;
			}
			else
			{
				this.SpellLevel = 0;
			}
			yield break;
		}
		public override void OnSpawn(EnemyUnit spawner)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				this.React(new ApplyStatusEffectAction<KoishiUnknown>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true));
				this.SpellLevel = 1;
			}
			else
			{
				this.SpellLevel = 0;
			}
			this.React(new ApplyStatusEffectAction<MirrorImage>(this, default(int?), default(int?), default(int?), default(int?), 0f, true));
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Koishi.MoveType.TripleShoot:
				yield return base.AttackMove(base.GetMove(6), base.Gun1, base.Damage1, 3, false, "Instant", true);
				break;
			case Koishi.MoveType.DoubleShoot:
				yield return base.AttackMove(base.GetMove(7), base.Gun2, base.Damage2, 2, false, "Instant", true);
				break;
			case Koishi.MoveType.ShootDefend:
				yield return base.AttackMove(base.GetMove(8), base.Gun3, base.Damage1, 1, false, "Instant", true);
				yield return base.DefendMove(this, null, base.Defend, 0, 0, true, null);
				break;
			case Koishi.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCardName, new int?(base.Damage4), true), this.SpellActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (this.Next == Koishi.MoveType.Spell)
			{
				yield break;
			}
			switch (this.NextInspiration)
			{
			case Koishi.InspirationType.White:
				yield return base.DefendMove(this, base.GetMove(1), 0, this.ShieldCount, 0, true, PerformAction.Effect(this, "InspirationW", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f)).AsHiddenIntention(this.HiddenIntention);
				break;
			case Koishi.InspirationType.Blue:
				yield return base.AddCardMove(base.GetMove(2), Library.CreateCards<KoishiDiscard>(this.AddCardsCount, false), EnemyUnit.AddCardZone.Draw, PerformAction.Effect(this, "InspirationU", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f), false).AsHiddenIntention(this.HiddenIntention);
				break;
			case Koishi.InspirationType.Black:
			{
				string move = base.GetMove(3);
				Type typeFromHandle = typeof(Weak);
				int? num = new int?(this.DebuffDuration);
				PerformAction performAction = PerformAction.Effect(this, "InspirationB", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				yield return base.NegativeMove(move, typeFromHandle, default(int?), num, false, false, performAction).AsHiddenIntention(this.HiddenIntention);
				break;
			}
			case Koishi.InspirationType.Red:
			{
				string move2 = base.GetMove(4);
				Type typeFromHandle2 = typeof(Firepower);
				int? num2 = new int?(this.PowerCount);
				PerformAction performAction = PerformAction.Effect(this, "InspirationR", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
				int? num = default(int?);
				yield return base.PositiveMove(move2, typeFromHandle2, num2, num, false, performAction).AsHiddenIntention(this.HiddenIntention);
				break;
			}
			case Koishi.InspirationType.Green:
				yield return base.DefendMove(this, base.GetMove(5), 0, 0, this.GrazeCount, true, PerformAction.Effect(this, "InspirationG", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f)).AsHiddenIntention(this.HiddenIntention);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (base.CountDown == 1)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun4, base.Damage4, 1, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			int num = this.SpellLevel + 1;
			this.SpellLevel = num;
			yield return new ApplyStatusEffectAction<KoishiUnknown>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
			yield break;
		}
		private int SpellLevel { get; set; }
		private bool HiddenIntention
		{
			get
			{
				return this.SpellLevel > 0;
			}
		}
		private int ShieldCount
		{
			get
			{
				return base.Defend / 2 + this.SpellLevel * 2;
			}
		}
		private int AddCardsCount
		{
			get
			{
				if (this.SpellLevel <= 1)
				{
					return 1;
				}
				return 2;
			}
		}
		private int DebuffDuration
		{
			get
			{
				if (this.SpellLevel <= 2)
				{
					return 1;
				}
				return 2;
			}
		}
		private int PowerCount
		{
			get
			{
				if (this.SpellLevel <= 3)
				{
					return 1;
				}
				return 2;
			}
		}
		private int GrazeCount
		{
			get
			{
				if (this.SpellLevel <= 2)
				{
					return 1;
				}
				return 2;
			}
		}
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Koishi.MoveType.Spell;
				base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
			}
			else
			{
				this.Last = this.Next;
				this.Next = this._pool.Without(this.Last).Sample(base.EnemyMoveRng);
			}
			this.PreviousInspiration = this.NextInspiration;
			this.NextInspiration = this._insPool.Without(this.PreviousInspiration).Sample(this.InspirationRng);
		}
		private readonly RepeatableRandomPool<Koishi.MoveType> _pool = new RepeatableRandomPool<Koishi.MoveType>
		{
			{
				Koishi.MoveType.TripleShoot,
				1f
			},
			{
				Koishi.MoveType.DoubleShoot,
				1f
			},
			{
				Koishi.MoveType.ShootDefend,
				1f
			}
		};
		private readonly RepeatableRandomPool<Koishi.InspirationType> _insPool = new RepeatableRandomPool<Koishi.InspirationType>
		{
			{
				Koishi.InspirationType.White,
				1f
			},
			{
				Koishi.InspirationType.Blue,
				1f
			},
			{
				Koishi.InspirationType.Black,
				1f
			},
			{
				Koishi.InspirationType.Red,
				1.5f
			},
			{
				Koishi.InspirationType.Green,
				1f
			}
		};
		private enum MoveType
		{
			TripleShoot,
			DoubleShoot,
			ShootDefend,
			Spell
		}
		private enum InspirationType
		{
			White,
			Blue,
			Black,
			Red,
			Green
		}
	}
}
