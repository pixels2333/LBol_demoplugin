using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x0200024C RID: 588
	[UsedImplicitly]
	public sealed class Tianzi : EnemyUnit
	{
		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000964 RID: 2404 RVA: 0x000144B5 File Offset: 0x000126B5
		// (set) Token: 0x06000965 RID: 2405 RVA: 0x000144BD File Offset: 0x000126BD
		private Tianzi.MoveType Next { get; set; }

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000966 RID: 2406 RVA: 0x000144C6 File Offset: 0x000126C6
		private string SpellBuff
		{
			get
			{
				return base.GetSpellCardName(new int?(1), 2);
			}
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000967 RID: 2407 RVA: 0x000144D8 File Offset: 0x000126D8
		private string SpellAttack
		{
			get
			{
				return base.GetSpellCardName(default(int?), 0);
			}
		}

		// Token: 0x06000968 RID: 2408 RVA: 0x000144F5 File Offset: 0x000126F5
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Tianzi.MoveType.AttackAndDebuff;
			this.DebuffCountDown = 4;
			base.CountDown = 2;
			this.SpellAttackTimes = 0;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x06000969 RID: 2409 RVA: 0x00014530 File Offset: 0x00012730
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<FlatPeach>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<EnemyEnergy>(this, new int?(0), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x0600096A RID: 2410 RVA: 0x00014540 File Offset: 0x00012740
		private IEnumerable<BattleAction> DebuffActions()
		{
			Unit player = base.Battle.Player;
			int? num = new int?(2);
			yield return new ApplyStatusEffectAction<Vulnerable>(player, default(int?), num, default(int?), default(int?), 0f, false);
			Unit player2 = base.Battle.Player;
			num = new int?(2);
			yield return new ApplyStatusEffectAction<Fragil>(player2, default(int?), num, default(int?), default(int?), 0f, false);
			yield break;
		}

		// Token: 0x0600096B RID: 2411 RVA: 0x00014550 File Offset: 0x00012750
		private IEnumerable<BattleAction> SpellBuffActions()
		{
			if (this._spellBuffShowed)
			{
				yield return new EnemyMoveAction(this, this.SpellBuff, true);
			}
			else
			{
				yield return PerformAction.Spell(this, "无念无想的境界");
				this._spellBuffShowed = true;
			}
			yield return PerformAction.Animation(this, "defend", 0.2f, null, 0f, -1);
			int? num = new int?(1);
			yield return new ApplyStatusEffectAction<Invincible>(this, default(int?), num, default(int?), default(int?), 0.2f, true);
			int? num2 = new int?(base.Power);
			num = default(int?);
			int? num3 = num;
			num = default(int?);
			int? num4 = num;
			num = default(int?);
			yield return new ApplyStatusEffectAction<Firepower>(this, num2, num3, num4, num, 0.2f, true);
			yield break;
		}

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x0600096C RID: 2412 RVA: 0x00014560 File Offset: 0x00012760
		// (set) Token: 0x0600096D RID: 2413 RVA: 0x00014568 File Offset: 0x00012768
		private int SpellAttackTimes { get; set; }

		// Token: 0x0600096E RID: 2414 RVA: 0x00014571 File Offset: 0x00012771
		private IEnumerable<BattleAction> SpellAttackActions()
		{
			yield return new ApplyStatusEffectAction<EnemyEnergyNegative>(this, new int?(100), default(int?), default(int?), default(int?), 0f, true);
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun3, base.Damage3 + base.Count2 * this.SpellAttackTimes, 1, true, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			int num = this.SpellAttackTimes + 1;
			this.SpellAttackTimes = num;
			yield break;
			yield break;
		}

		// Token: 0x0600096F RID: 2415 RVA: 0x00014581 File Offset: 0x00012781
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Tianzi.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(3), base.Gun1, base.Damage1, 3, false, "Instant", true);
				break;
			case Tianzi.MoveType.AttackAndDebuff:
				yield return base.AttackMove(base.GetMove(4), base.Gun2, base.Damage2, 2, false, "Instant", true);
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null), this.DebuffActions());
				break;
			case Tianzi.MoveType.DefendAndBuff:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellBuff, default(int?), default(int?), false), this.SpellBuffActions());
				break;
			case Tianzi.MoveType.SpellAttack:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellAttack, new int?(base.Damage3 + base.Count2 * this.SpellAttackTimes), true), this.SpellAttackActions());
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000970 RID: 2416 RVA: 0x00014591 File Offset: 0x00012791
		// (set) Token: 0x06000971 RID: 2417 RVA: 0x00014599 File Offset: 0x00012799
		private int DebuffCountDown { get; set; }

		// Token: 0x06000972 RID: 2418 RVA: 0x000145A4 File Offset: 0x000127A4
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			num = this.DebuffCountDown - 1;
			this.DebuffCountDown = num;
			if (base.CountDown <= 0)
			{
				this.Next = Tianzi.MoveType.DefendAndBuff;
				base.CountDown = 5;
				return;
			}
			EnemyEnergy statusEffect = base.GetStatusEffect<EnemyEnergy>();
			if (statusEffect != null && statusEffect.Level >= 100)
			{
				this.Next = Tianzi.MoveType.SpellAttack;
				return;
			}
			if (this.DebuffCountDown <= 0)
			{
				this.Next = Tianzi.MoveType.AttackAndDebuff;
				this.DebuffCountDown = 4;
				return;
			}
			this.Next = Tianzi.MoveType.Shoot;
		}

		// Token: 0x040000DB RID: 219
		private const int EnergyUse = 100;

		// Token: 0x040000DC RID: 220
		private bool _spellBuffShowed;

		// Token: 0x02000782 RID: 1922
		private enum MoveType
		{
			// Token: 0x04000BD4 RID: 3028
			Shoot,
			// Token: 0x04000BD5 RID: 3029
			AttackAndDebuff,
			// Token: 0x04000BD6 RID: 3030
			DefendAndBuff,
			// Token: 0x04000BD7 RID: 3031
			SpellAttack
		}
	}
}
