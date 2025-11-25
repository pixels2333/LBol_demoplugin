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
	[UsedImplicitly]
	public sealed class Tianzi : EnemyUnit
	{
		private Tianzi.MoveType Next { get; set; }
		private string SpellBuff
		{
			get
			{
				return base.GetSpellCardName(new int?(1), 2);
			}
		}
		private string SpellAttack
		{
			get
			{
				return base.GetSpellCardName(default(int?), 0);
			}
		}
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = Tianzi.MoveType.AttackAndDebuff;
			this.DebuffCountDown = 4;
			base.CountDown = 2;
			this.SpellAttackTimes = 0;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<FlatPeach>(this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield return new ApplyStatusEffectAction<EnemyEnergy>(this, new int?(0), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
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
		private int SpellAttackTimes { get; set; }
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
		private int DebuffCountDown { get; set; }
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
		private const int EnergyUse = 100;
		private bool _spellBuffShowed;
		private enum MoveType
		{
			Shoot,
			AttackAndDebuff,
			DefendAndBuff,
			SpellAttack
		}
	}
}
