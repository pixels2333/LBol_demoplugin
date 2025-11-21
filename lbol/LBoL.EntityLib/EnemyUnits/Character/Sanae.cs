using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000246 RID: 582
	[UsedImplicitly]
	public sealed class Sanae : EnemyUnit
	{
		// Token: 0x17000100 RID: 256
		// (get) Token: 0x06000925 RID: 2341 RVA: 0x00013D09 File Offset: 0x00011F09
		// (set) Token: 0x06000926 RID: 2342 RVA: 0x00013D11 File Offset: 0x00011F11
		private Sanae.MoveType Last { get; set; }

		// Token: 0x17000101 RID: 257
		// (get) Token: 0x06000927 RID: 2343 RVA: 0x00013D1A File Offset: 0x00011F1A
		// (set) Token: 0x06000928 RID: 2344 RVA: 0x00013D22 File Offset: 0x00011F22
		private Sanae.MoveType Next { get; set; }

		// Token: 0x17000102 RID: 258
		// (get) Token: 0x06000929 RID: 2345 RVA: 0x00013D2B File Offset: 0x00011F2B
		private string MultiShootMove
		{
			get
			{
				return base.GetSpellCardName(new int?(0), 1);
			}
		}

		// Token: 0x17000103 RID: 259
		// (get) Token: 0x0600092A RID: 2346 RVA: 0x00013D3A File Offset: 0x00011F3A
		private string ShootAccuracyMove
		{
			get
			{
				return base.GetSpellCardName(new int?(2), 3);
			}
		}

		// Token: 0x17000104 RID: 260
		// (get) Token: 0x0600092B RID: 2347 RVA: 0x00013D49 File Offset: 0x00011F49
		private string HealMove
		{
			get
			{
				return base.GetSpellCardName(new int?(4), 5);
			}
		}

		// Token: 0x17000105 RID: 261
		// (get) Token: 0x0600092C RID: 2348 RVA: 0x00013D58 File Offset: 0x00011F58
		private string Spell
		{
			get
			{
				return base.GetSpellCardName(new int?(6), 7);
			}
		}

		// Token: 0x0600092D RID: 2349 RVA: 0x00013D67 File Offset: 0x00011F67
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Sanae.MoveType.Spell;
			this.Last = Sanae.MoveType.ShootAccuracy;
			this.HealCountDown = 0;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x0600092E RID: 2350 RVA: 0x00013D9B File Offset: 0x00011F9B
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<Curiosity>(this, new int?((base.Difficulty == GameDifficulty.Lunatic) ? 2 : 1), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<Amulet>(this, new int?(3), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600092F RID: 2351 RVA: 0x00013DAB File Offset: 0x00011FAB
		private IEnumerable<BattleAction> SpellActions()
		{
			yield return PerformAction.Spell(this, "灵力掠夺者");
			yield return PerformAction.Animation(this, "defend", 0.5f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<SpiritNegative>(base.Battle.Player, new int?(2), default(int?), default(int?), default(int?), 0.3f, true);
			yield return new ApplyStatusEffectAction<Spirit>(this, new int?(2), default(int?), default(int?), default(int?), 0.3f, true);
			yield return new CastBlockShieldAction(this, base.Defend, base.Defend, BlockShieldType.Normal, false);
			base.CountDown = base.EnemyMoveRng.NextInt(4, 5);
			yield break;
		}

		// Token: 0x06000930 RID: 2352 RVA: 0x00013DBB File Offset: 0x00011FBB
		private IEnumerable<BattleAction> HealActions()
		{
			yield return new EnemyMoveAction(this, this.HealMove, true);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new HealAction(this, this, base.Count1, HealType.Normal, 0.2f);
			yield return new ApplyStatusEffectAction<Amulet>(this, new int?(2), default(int?), default(int?), default(int?), 0f, true);
			this.HealCountDown = base.EnemyMoveRng.NextInt(4, 5);
			yield break;
		}

		// Token: 0x06000931 RID: 2353 RVA: 0x00013DCB File Offset: 0x00011FCB
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Sanae.MoveType.MultiShoot:
			{
				yield return base.AttackMove(this.MultiShootMove, base.Gun1, base.Damage1, 4, false, "InstantSpeed.3", true);
				string text = null;
				Type typeFromHandle = typeof(Weak);
				int? num = new int?((base.Difficulty == GameDifficulty.Lunatic) ? 3 : 2);
				yield return base.NegativeMove(text, typeFromHandle, default(int?), num, true, false, null);
				break;
			}
			case Sanae.MoveType.ShootAccuracy:
				yield return base.AttackMove(this.ShootAccuracyMove, base.Gun2, base.Damage2, 1, true, "Instant", true);
				yield return base.AddCardMove(null, typeof(Yueguang), (base.Difficulty == GameDifficulty.Lunatic) ? 3 : 2, EnemyUnit.AddCardZone.Discard, null, false);
				break;
			case Sanae.MoveType.Heal:
				yield return new SimpleEnemyMove(Intention.Heal().WithMoveName(this.HealMove), this.HealActions());
				yield return base.ClearMove(0f);
				break;
			case Sanae.MoveType.Spell:
			{
				string spell = this.Spell;
				int? num = default(int?);
				int? num2 = num;
				num = default(int?);
				yield return new SimpleEnemyMove(Intention.SpellCard(spell, num2, num, false), this.SpellActions());
				break;
			}
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

		// Token: 0x17000106 RID: 262
		// (get) Token: 0x06000932 RID: 2354 RVA: 0x00013DDB File Offset: 0x00011FDB
		// (set) Token: 0x06000933 RID: 2355 RVA: 0x00013DE3 File Offset: 0x00011FE3
		private int HealCountDown { get; set; }

		// Token: 0x06000934 RID: 2356 RVA: 0x00013DEC File Offset: 0x00011FEC
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Sanae.MoveType.Spell;
				return;
			}
			if (base.Hp <= base.MaxHp / 2)
			{
				num = this.HealCountDown - 1;
				this.HealCountDown = num;
				if (this.HealCountDown <= 0)
				{
					this.Next = Sanae.MoveType.Heal;
					return;
				}
			}
			this.Next = ((this.Last == Sanae.MoveType.MultiShoot) ? Sanae.MoveType.ShootAccuracy : Sanae.MoveType.MultiShoot);
			this.Last = this.Next;
		}

		// Token: 0x02000769 RID: 1897
		private enum MoveType
		{
			// Token: 0x04000B66 RID: 2918
			MultiShoot,
			// Token: 0x04000B67 RID: 2919
			ShootAccuracy,
			// Token: 0x04000B68 RID: 2920
			Heal,
			// Token: 0x04000B69 RID: 2921
			Spell
		}
	}
}
