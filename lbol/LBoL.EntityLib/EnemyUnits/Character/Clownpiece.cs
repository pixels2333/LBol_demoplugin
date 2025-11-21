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
	// Token: 0x02000237 RID: 567
	[UsedImplicitly]
	public sealed class Clownpiece : EnemyUnit
	{
		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x0600088D RID: 2189 RVA: 0x00012A45 File Offset: 0x00010C45
		// (set) Token: 0x0600088E RID: 2190 RVA: 0x00012A4D File Offset: 0x00010C4D
		private Clownpiece.MoveType Next { get; set; }

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x0600088F RID: 2191 RVA: 0x00012A58 File Offset: 0x00010C58
		private string Spell
		{
			get
			{
				return base.GetSpellCardName(default(int?), 2);
			}
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x00012A75 File Offset: 0x00010C75
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Clownpiece.MoveType.Spawn;
			this._summonLevel = ((base.GameRun.Difficulty == GameDifficulty.Lunatic) ? 1 : 0);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x00012AB3 File Offset: 0x00010CB3
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<LunaticTorch>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield return new CastBlockShieldAction(this, this, 0, base.Count1, BlockShieldType.Normal, false);
			yield break;
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x00012AC3 File Offset: 0x00010CC3
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

		// Token: 0x06000893 RID: 2195 RVA: 0x00012AD3 File Offset: 0x00010CD3
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

		// Token: 0x06000894 RID: 2196 RVA: 0x00012AE4 File Offset: 0x00010CE4
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			this.Next = ((base.CountDown <= 0) ? Clownpiece.MoveType.Spawn : this._pool.Sample(base.EnemyMoveRng));
		}

		// Token: 0x0400009C RID: 156
		private int _summonLevel;

		// Token: 0x0400009D RID: 157
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

		// Token: 0x0200071F RID: 1823
		private enum MoveType
		{
			// Token: 0x04000A0C RID: 2572
			DoubleShoot,
			// Token: 0x04000A0D RID: 2573
			ShootAccuracy,
			// Token: 0x04000A0E RID: 2574
			Spawn
		}
	}
}
