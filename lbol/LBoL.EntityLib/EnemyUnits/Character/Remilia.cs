using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Randoms;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x02000243 RID: 579
	[UsedImplicitly]
	public sealed class Remilia : EnemyUnit
	{
		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x06000903 RID: 2307 RVA: 0x000137A5 File Offset: 0x000119A5
		// (set) Token: 0x06000904 RID: 2308 RVA: 0x000137AD File Offset: 0x000119AD
		private Remilia.MoveType Next { get; set; }

		// Token: 0x170000FA RID: 250
		// (get) Token: 0x06000905 RID: 2309 RVA: 0x000137B6 File Offset: 0x000119B6
		private string SpellGungnir
		{
			get
			{
				return base.GetSpellCardName(new int?(4), 5);
			}
		}

		// Token: 0x170000FB RID: 251
		// (get) Token: 0x06000906 RID: 2310 RVA: 0x000137C5 File Offset: 0x000119C5
		private string SpellQueen
		{
			get
			{
				return base.GetSpellCardName(new int?(6), 7);
			}
		}

		// Token: 0x170000FC RID: 252
		// (get) Token: 0x06000907 RID: 2311 RVA: 0x000137D4 File Offset: 0x000119D4
		private string SpellDevil
		{
			get
			{
				return base.GetSpellCardName(new int?(8), 9);
			}
		}

		// Token: 0x06000908 RID: 2312 RVA: 0x000137E4 File Offset: 0x000119E4
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Remilia.MoveType.MultiAttack;
			base.CountDown = 1;
			this._gungnirSpellShowed = false;
			this._lastSe = typeof(Vulnerable);
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.HandleBattleEvent<DamageEventArgs>(base.DamageReceived, new GameEventHandler<DamageEventArgs>(this.OnDamageReceived));
		}

		// Token: 0x06000909 RID: 2313 RVA: 0x0001384B File Offset: 0x00011A4B
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			int? num = new int?(base.Count2);
			yield return new ApplyStatusEffectAction<ScarletDestiny>(this, default(int?), default(int?), default(int?), num, 0f, true);
			int? num2 = new int?(3);
			num = default(int?);
			int? num3 = num;
			num = default(int?);
			int? num4 = num;
			num = default(int?);
			yield return new ApplyStatusEffectAction<Graze>(this, num2, num3, num4, num, 0f, true);
			yield break;
		}

		// Token: 0x0600090A RID: 2314 RVA: 0x0001385B File Offset: 0x00011A5B
		private void OnDamageReceived(DamageEventArgs args)
		{
			if (!base.HasStatusEffect<Vampire>() && base.Hp <= base.MaxHp / 2)
			{
				if (base.IsInTurn)
				{
					this._nextDevil = true;
					return;
				}
				this.Next = Remilia.MoveType.SpellVampire;
				base.UpdateTurnMoves();
			}
		}

		// Token: 0x0600090B RID: 2315 RVA: 0x00013892 File Offset: 0x00011A92
		private IEnumerable<BattleAction> GungnirActions()
		{
			if (this._gungnirSpellShowed)
			{
				yield return new EnemyMoveAction(this, this.SpellGungnir, true);
			}
			else
			{
				yield return PerformAction.Spell(this, "Spear the Gungnir");
				this._gungnirSpellShowed = true;
			}
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun3, base.Damage3, 1, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x0600090C RID: 2316 RVA: 0x000138A2 File Offset: 0x00011AA2
		private IEnumerable<BattleAction> MidnightActions()
		{
			yield return PerformAction.Spell(this, "Queen of Midnight");
			yield return PerformAction.Animation(this, "skill1", 0.5f, null, 0f, -1);
			yield return new ApplyStatusEffectAction<Graze>(this, new int?(base.Defend), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			base.CountDown = 4;
			yield break;
		}

		// Token: 0x0600090D RID: 2317 RVA: 0x000138B2 File Offset: 0x00011AB2
		private IEnumerable<BattleAction> DevilActions()
		{
			yield return PerformAction.Spell(this, "Scarlet Devil");
			yield return PerformAction.Animation(this, "cast3", 0.5f, null, 0f, -1);
			yield return PerformAction.Effect(this, "RenzhenAura", 0f, "RenzhenAura", 0f, PerformAction.EffectBehavior.Add, 0f);
			yield return new HealAction(this, this, 50, HealType.Normal, 0.2f);
			int? num = default(int?);
			int? num2 = num;
			num = default(int?);
			int? num3 = num;
			num = default(int?);
			int? num4 = num;
			num = default(int?);
			yield return new ApplyStatusEffectAction<Vampire>(this, num2, num3, num4, num, 0f, true);
			num = new int?(1);
			yield return new ApplyStatusEffectAction<Invincible>(this, default(int?), num, default(int?), default(int?), 0f, true);
			int? num5 = new int?(base.Power);
			num = default(int?);
			int? num6 = num;
			num = default(int?);
			int? num7 = num;
			num = default(int?);
			yield return new ApplyStatusEffectAction<Firepower>(this, num5, num6, num7, num, 0f, true);
			yield break;
		}

		// Token: 0x0600090E RID: 2318 RVA: 0x000138C2 File Offset: 0x00011AC2
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Remilia.MoveType.MultiAttack:
			{
				this._nextSe = this._negativeSePool.Without(this._lastSe).Sample(base.EnemyBattleRng);
				this._lastSe = this._nextSe;
				int? num = default(int?);
				yield return base.AttackMove(base.GetSpellCardName(num, 2), base.Gun1, base.Damage1, 4, false, "Instant", true);
				string text = null;
				Type nextSe = this._nextSe;
				num = new int?(base.Count1);
				yield return base.NegativeMove(text, nextSe, default(int?), num, false, false, null);
				break;
			}
			case Remilia.MoveType.ShootGraze:
			{
				int? num = default(int?);
				yield return base.AttackMove(base.GetSpellCardName(num, 3), base.Gun2, base.Damage2, 2, false, "Instant", true);
				yield return base.DefendMove(this, null, 0, 0, base.Defend, false, null);
				break;
			}
			case Remilia.MoveType.ShootAccuracy:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellGungnir, new int?(base.Damage3), true), this.GungnirActions());
				break;
			case Remilia.MoveType.GrazeBuff:
			{
				string spellQueen = this.SpellQueen;
				int? num = default(int?);
				int? num2 = num;
				num = default(int?);
				yield return new SimpleEnemyMove(Intention.SpellCard(spellQueen, num2, num, false), this.MidnightActions());
				yield return base.ClearMove(0f);
				base.CountDown = 5;
				break;
			}
			case Remilia.MoveType.SpellVampire:
			{
				string spellDevil = this.SpellDevil;
				int? num = default(int?);
				int? num3 = num;
				num = default(int?);
				yield return new SimpleEnemyMove(Intention.SpellCard(spellDevil, num3, num, false), this.DevilActions());
				yield return base.ClearMove(0f);
				base.CountDown = 3;
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (base.CountDown == 1)
			{
				yield return new SimpleEnemyMove(Intention.CountDown(base.CountDown));
			}
			yield break;
		}

		// Token: 0x0600090F RID: 2319 RVA: 0x000138D4 File Offset: 0x00011AD4
		protected override void UpdateMoveCounters()
		{
			if (this._nextDevil)
			{
				this.Next = Remilia.MoveType.SpellVampire;
				this._nextDevil = false;
				return;
			}
			int num = base.CountDown - 1;
			base.CountDown = num;
			if (base.CountDown < 0)
			{
				this.Next = Remilia.MoveType.GrazeBuff;
				return;
			}
			if (base.CountDown == 0)
			{
				this.Next = Remilia.MoveType.ShootAccuracy;
				return;
			}
			this.Next = ((base.CountDown % 2 == 1) ? Remilia.MoveType.MultiAttack : Remilia.MoveType.ShootGraze);
		}

		// Token: 0x040000C2 RID: 194
		private bool _nextDevil;

		// Token: 0x040000C3 RID: 195
		private bool _gungnirSpellShowed;

		// Token: 0x040000C4 RID: 196
		private Type _lastSe;

		// Token: 0x040000C5 RID: 197
		private Type _nextSe;

		// Token: 0x040000C6 RID: 198
		private readonly RepeatableRandomPool<Type> _negativeSePool = new RepeatableRandomPool<Type>
		{
			{
				typeof(Vulnerable),
				1f
			},
			{
				typeof(Fragil),
				1f
			},
			{
				typeof(Weak),
				1f
			}
		};

		// Token: 0x02000758 RID: 1880
		private enum MoveType
		{
			// Token: 0x04000B12 RID: 2834
			MultiAttack,
			// Token: 0x04000B13 RID: 2835
			ShootGraze,
			// Token: 0x04000B14 RID: 2836
			ShootAccuracy,
			// Token: 0x04000B15 RID: 2837
			GrazeBuff,
			// Token: 0x04000B16 RID: 2838
			SpellVampire
		}
	}
}
