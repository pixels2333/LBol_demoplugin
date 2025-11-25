using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.StatusEffects.Enemy;
using UnityEngine;
namespace LBoL.EntityLib.EnemyUnits.Character
{
	[UsedImplicitly]
	public sealed class Clownpiece : EnemyUnit
	{
		private Clownpiece.MoveType Next { get; set; }
		private string Spell
		{
			get
			{
				return base.GetSpellCardName(default(int?), 2);
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Clownpiece.MoveType.Spawn;
			this._summonLevel = ((base.GameRun.Difficulty == GameDifficulty.Lunatic) ? 1 : 0);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<LunaticTorch>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield return new CastBlockShieldAction(this, this, 0, base.Count1, BlockShieldType.Normal, false);
			yield break;
		}
		private IEnumerable<BattleAction> SpellActions()
		{
			yield return PerformAction.Animation(this, "spell", 0f, null, 0f, -1);
			yield return PerformAction.Spell(this, "妖精大暴走");
			int num = this._summonLevel;
			List<Type> list3;
			if (num != 0)
			{
				if (num != 1)
				{
					List<Type> list2 = new List<Type>();
					list2.Add(typeof(BlackFairy));
					list2.Add(typeof(BlackFairy));
					list2.Add(typeof(BlackFairy));
					list3 = list2;
				}
				else
				{
					List<Type> list4 = new List<Type>();
					list4.Add(typeof(WhiteFairy));
					list4.Add(typeof(BlackFairy));
					list4.Add(typeof(BlackFairy));
					list3 = list4;
				}
			}
			else
			{
				List<Type> list5 = new List<Type>();
				list5.Add(typeof(BlackFairy));
				list5.Add(typeof(WhiteFairy));
				list5.Add(typeof(WhiteFairy));
				list3 = list5;
			}
			List<Type> list = list3;
			Clownpiece.<>c__DisplayClass9_0 CS$<>8__locals1 = new Clownpiece.<>c__DisplayClass9_0();
			CS$<>8__locals1.i = 0;
			while (CS$<>8__locals1.i < 3)
			{
				List<EnemyUnit> list6 = Enumerable.ToList<EnemyUnit>(Enumerable.Where<EnemyUnit>(base.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex == CS$<>8__locals1.i));
				if (list6.Count > 0)
				{
					yield return new HealAction(this, Enumerable.First<EnemyUnit>(list6), 999, HealType.Normal, 0.2f);
				}
				else
				{
					yield return new SpawnEnemyAction(this, list[CS$<>8__locals1.i], CS$<>8__locals1.i, 0f, 0.3f, true);
				}
				num = CS$<>8__locals1.i;
				CS$<>8__locals1.i = num + 1;
			}
			CS$<>8__locals1 = null;
			this._summonLevel++;
			base.CountDown = 4;
			string text = "Chat.Clownpiece";
			text += Random.Range(1, 4).ToString();
			yield return PerformAction.Chat(this, text.Localize(true), 3f, 0f, 1f, true);
			yield break;
		}
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Clownpiece.MoveType.DoubleShoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 3, false, "Instant", false);
				yield return base.AddCardMove(null, Library.CreateCards<LBoL.EntityLib.Cards.Enemy.Lunatic>(1, false), EnemyUnit.AddCardZone.Draw, null, false);
				break;
			case Clownpiece.MoveType.ShootAccuracy:
				yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 1, true, "Instant", false);
				yield return base.AddCardMove(null, Library.CreateCards<LBoL.EntityLib.Cards.Enemy.Lunatic>(1, false), EnemyUnit.AddCardZone.Draw, null, false);
				break;
			case Clownpiece.MoveType.Spawn:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.Spell, default(int?), default(int?), false), this.SpellActions());
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
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Next = ((base.CountDown <= 0) ? Clownpiece.MoveType.Spawn : this._pool.Sample(base.EnemyMoveRng));
		}
		private int _summonLevel;
		private readonly RepeatableRandomPool<Clownpiece.MoveType> _pool = new RepeatableRandomPool<Clownpiece.MoveType>
		{
			{
				Clownpiece.MoveType.DoubleShoot,
				1f
			},
			{
				Clownpiece.MoveType.ShootAccuracy,
				1f
			}
		};
		private enum MoveType
		{
			DoubleShoot,
			ShootAccuracy,
			Spawn
		}
	}
}
