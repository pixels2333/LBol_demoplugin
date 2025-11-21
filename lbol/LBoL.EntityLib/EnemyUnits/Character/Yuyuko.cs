using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Character
{
	// Token: 0x0200024F RID: 591
	[UsedImplicitly]
	public sealed class Yuyuko : EnemyUnit<IRinView>
	{
		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000984 RID: 2436 RVA: 0x000147A8 File Offset: 0x000129A8
		// (set) Token: 0x06000985 RID: 2437 RVA: 0x000147B0 File Offset: 0x000129B0
		private Yuyuko.MoveType Next { get; set; }

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x06000986 RID: 2438 RVA: 0x000147B9 File Offset: 0x000129B9
		private string SpellCard
		{
			get
			{
				return base.GetSpellCardName(new int?(5), 6);
			}
		}

		// Token: 0x06000987 RID: 2439 RVA: 0x000147C8 File Offset: 0x000129C8
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				this.Next = Yuyuko.MoveType.Shoot;
				base.CountDown = 1;
				this._attackWithDebuffTurn = 3;
			}
			else
			{
				this.Next = Yuyuko.MoveType.Summon;
				base.CountDown = 2;
				this._attackWithDebuffTurn = 4;
			}
			this._debuffCount = 2;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			IRinView view = base.View;
			if (view != null)
			{
				view.SetOrb("YuyukoTrail", 0);
			}
			IRinView view2 = base.View;
			if (view2 == null)
			{
				return;
			}
			view2.SetOrb("YuyukoTrail", 1);
		}

		// Token: 0x06000988 RID: 2440 RVA: 0x0001485B File Offset: 0x00012A5B
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			if (base.Difficulty == GameDifficulty.Lunatic)
			{
				foreach (BattleAction battleAction in this.SummonActions())
				{
					yield return battleAction;
				}
				IEnumerator<BattleAction> enumerator = null;
			}
			if (base.GameRun.ExtraFlags.Contains("YoumuMooncake"))
			{
				SadinYuebing sadinYuebing = Library.CreateExhibit<SadinYuebing>();
				string text = "Chat.YuyukoMooncake".LocalizeFormat(new object[] { sadinYuebing.GetName() });
				yield return PerformAction.Chat(this, text, 3f, 1f, 0f, true);
			}
			yield break;
			yield break;
		}

		// Token: 0x06000989 RID: 2441 RVA: 0x0001486B File Offset: 0x00012A6B
		private IEnumerable<BattleAction> DefendBuff()
		{
			yield return new EnemyMoveAction(this, base.GetMove(2), true);
			bool flag = true;
			foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup.Alives)
			{
				yield return new CastBlockShieldAction(enemyUnit, base.Defend, 0, BlockShieldType.Normal, flag);
				flag = false;
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x0600098A RID: 2442 RVA: 0x0001487B File Offset: 0x00012A7B
		private IEnumerable<BattleAction> Buff()
		{
			foreach (EnemyUnit enemyUnit in base.Battle.EnemyGroup.Alives)
			{
				yield return new ApplyStatusEffectAction<Firepower>(enemyUnit, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			}
			IEnumerator<EnemyUnit> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x0600098B RID: 2443 RVA: 0x0001488B File Offset: 0x00012A8B
		private IEnumerable<BattleAction> DebuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(3), true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			Type typeFromHandle = typeof(Weak);
			Unit player = base.Battle.Player;
			int? num = new int?(3);
			yield return new ApplyStatusEffectAction(typeFromHandle, player, default(int?), num, default(int?), default(int?), 0f, false);
			yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
			Type typeFromHandle2 = typeof(Fragil);
			Unit player2 = base.Battle.Player;
			num = new int?(3);
			yield return new ApplyStatusEffectAction(typeFromHandle2, player2, default(int?), num, default(int?), default(int?), 0.3f, false);
			yield return PerformAction.Animation(base.Battle.Player, "Hit", 0.3f, null, 0f, -1);
			yield break;
		}

		// Token: 0x0600098C RID: 2444 RVA: 0x0001489B File Offset: 0x00012A9B
		private IEnumerable<BattleAction> SummonActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(4), true);
			yield return PerformAction.Animation(this, "shoot2", 0.5f, null, 0f, -1);
			Yuyuko.<>c__DisplayClass14_0 CS$<>8__locals1 = new Yuyuko.<>c__DisplayClass14_0();
			CS$<>8__locals1.i = 0;
			while (CS$<>8__locals1.i < 2)
			{
				if (Enumerable.All<EnemyUnit>(base.Battle.AllAliveEnemies, (EnemyUnit enemy) => enemy.RootIndex != CS$<>8__locals1.i))
				{
					yield return new SpawnEnemyAction<Wangling>(this, CS$<>8__locals1.i, 0f, 0.3f, true);
				}
				int i = CS$<>8__locals1.i;
				CS$<>8__locals1.i = i + 1;
			}
			CS$<>8__locals1 = null;
			this._summonTurn = 0;
			this._vacancy = 4;
			yield break;
		}

		// Token: 0x0600098D RID: 2445 RVA: 0x000148AB File Offset: 0x00012AAB
		private IEnumerable<BattleAction> SpellActions()
		{
			foreach (BattleAction battleAction in this.AttackActions(null, base.Gun3, base.Damage3, 1, false, "Instant"))
			{
				yield return battleAction;
			}
			IEnumerator<BattleAction> enumerator = null;
			yield return new ApplyStatusEffectAction(typeof(YuyukoDeath), base.Battle.Player, new int?(3), default(int?), default(int?), default(int?), 0.2f, true);
			base.CountDown = 4;
			yield break;
			yield break;
		}

		// Token: 0x0600098E RID: 2446 RVA: 0x000148BB File Offset: 0x00012ABB
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Yuyuko.MoveType.Shoot:
				if (this._shootIndicator == 0)
				{
					yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1, 3, false, "Instant", true);
					this._shootIndicator = 1;
				}
				else
				{
					yield return base.AttackMove(base.GetMove(1), base.Gun2, base.Damage2, 2, false, "Instant", true);
					this._shootIndicator = 0;
				}
				if (base.TurnCounter > this._attackWithDebuffTurn)
				{
					yield return base.NegativeMove(null, typeof(YuyukoDeath), new int?(1), default(int?), true, false, null);
				}
				break;
			case Yuyuko.MoveType.DefendBuff:
				yield return new SimpleEnemyMove(Intention.Defend().WithMoveName(base.GetMove(2)), this.DefendBuff());
				yield return new SimpleEnemyMove(Intention.PositiveEffect(), this.Buff());
				break;
			case Yuyuko.MoveType.Debuff:
				yield return new SimpleEnemyMove(Intention.NegativeEffect(null).WithMoveName(base.GetMove(3)), this.DebuffActions());
				this._debuffCount = 5;
				break;
			case Yuyuko.MoveType.Summon:
				yield return new SimpleEnemyMove(Intention.Spawn().WithMoveName(base.GetMove(4)), this.SummonActions());
				break;
			case Yuyuko.MoveType.Spell:
				yield return new SimpleEnemyMove(Intention.SpellCard(this.SpellCard, new int?(base.Damage3), default(int?), false), this.SpellActions());
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

		// Token: 0x0600098F RID: 2447 RVA: 0x000148CC File Offset: 0x00012ACC
		protected override void UpdateMoveCounters()
		{
			int num = base.CountDown - 1;
			base.CountDown = num;
			this._debuffCount--;
			int num2 = Enumerable.Count<EnemyUnit>(base.Battle.AllAliveEnemies);
			int num3 = 3 - num2;
			this._vacancy -= num3;
			if (base.CountDown <= 0)
			{
				this.Next = Yuyuko.MoveType.Spell;
				return;
			}
			if (this._vacancy <= 0)
			{
				this.Next = Yuyuko.MoveType.Summon;
				return;
			}
			this._summonTurn++;
			num = this._summonTurn % 3;
			Yuyuko.MoveType moveType;
			if (num != 1)
			{
				if (num != 2)
				{
					moveType = Yuyuko.MoveType.Shoot;
				}
				else
				{
					moveType = ((num2 >= 2) ? Yuyuko.MoveType.DefendBuff : Yuyuko.MoveType.Shoot);
				}
			}
			else
			{
				moveType = ((this._debuffCount <= 0) ? Yuyuko.MoveType.Debuff : Yuyuko.MoveType.Shoot);
			}
			this.Next = moveType;
		}

		// Token: 0x040000E2 RID: 226
		private int _attackWithDebuffTurn;

		// Token: 0x040000E3 RID: 227
		private int _vacancy;

		// Token: 0x040000E4 RID: 228
		private int _summonTurn;

		// Token: 0x040000E5 RID: 229
		private int _shootIndicator;

		// Token: 0x040000E6 RID: 230
		private int _debuffCount;

		// Token: 0x0200078D RID: 1933
		private enum MoveType
		{
			// Token: 0x04000C02 RID: 3074
			Shoot,
			// Token: 0x04000C03 RID: 3075
			DefendBuff,
			// Token: 0x04000C04 RID: 3076
			Debuff,
			// Token: 0x04000C05 RID: 3077
			Summon,
			// Token: 0x04000C06 RID: 3078
			Spell
		}
	}
}
