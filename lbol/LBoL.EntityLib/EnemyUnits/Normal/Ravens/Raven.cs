using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal.Ravens
{
	// Token: 0x020001F2 RID: 498
	[UsedImplicitly]
	public abstract class Raven : EnemyUnit
	{
		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x060007DC RID: 2012 RVA: 0x000117C4 File Offset: 0x0000F9C4
		// (set) Token: 0x060007DD RID: 2013 RVA: 0x000117CC File Offset: 0x0000F9CC
		private Raven.MoveType Next { get; set; }

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x060007DE RID: 2014 RVA: 0x000117D5 File Offset: 0x0000F9D5
		protected virtual bool HasGraze
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x060007DF RID: 2015 RVA: 0x000117D8 File Offset: 0x0000F9D8
		// (set) Token: 0x060007E0 RID: 2016 RVA: 0x000117E0 File Offset: 0x0000F9E0
		private int GrazeCountDown { get; set; }

		// Token: 0x060007E1 RID: 2017 RVA: 0x000117EC File Offset: 0x0000F9EC
		protected override void OnEnterBattle(BattleController battle)
		{
			if (base.RootIndex == 1)
			{
				Raven raven = null;
				foreach (EnemyUnit enemyUnit in base.AllAliveEnemies)
				{
					Raven raven2 = enemyUnit as Raven;
					if (raven2 != null)
					{
						raven = raven2;
						break;
					}
				}
				if (raven != null)
				{
					if (raven.Next == Raven.MoveType.AddCard)
					{
						this.Next = Raven.MoveType.Shoot;
						base.CountDown = 1;
					}
					else
					{
						this.Next = Raven.MoveType.AddCard;
					}
				}
				else
				{
					this.NormalStart();
				}
			}
			else
			{
				this.NormalStart();
			}
			if (this.HasGraze)
			{
				this.GrazeCountDown = base.EnemyMoveRng.NextInt(1, 3);
			}
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x000118B4 File Offset: 0x0000FAB4
		private void NormalStart()
		{
			if (base.EnemyMoveRng.NextInt(0, 1) == 0)
			{
				this.Next = Raven.MoveType.Shoot;
				base.CountDown = 1;
				return;
			}
			this.Next = Raven.MoveType.AddCard;
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x000118DB File Offset: 0x0000FADB
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction(typeof(Graze), this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}

		// Token: 0x060007E4 RID: 2020 RVA: 0x000118EC File Offset: 0x0000FAEC
		public override void OnSpawn(EnemyUnit spawner)
		{
			this.React(new ApplyStatusEffectAction(typeof(Graze), this, new int?(base.Count1), default(int?), default(int?), default(int?), 0f, true));
		}

		// Token: 0x060007E5 RID: 2021 RVA: 0x00011940 File Offset: 0x0000FB40
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			switch (this.Next)
			{
			case Raven.MoveType.Shoot:
				yield return base.AttackMove(base.GetMove(0), base.Gun1, base.Damage1 + base.EnemyBattleRng.NextInt(0, 2), 1, false, "Instant", false);
				break;
			case Raven.MoveType.AddCard:
				yield return new SimpleEnemyMove(Intention.AddCard(), this.News());
				base.CountDown = base.EnemyMoveRng.NextInt(2, 3);
				break;
			case Raven.MoveType.Graze:
				yield return base.DefendMove(this, base.GetMove(2), 0, 0, base.Defend, true, PerformAction.Animation(this, "defend", 0.3f, null, 0f, -1));
				this.GrazeCountDown = base.EnemyMoveRng.NextInt(3, 4);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			yield break;
		}

		// Token: 0x060007E6 RID: 2022 RVA: 0x00011950 File Offset: 0x0000FB50
		protected virtual IEnumerable<BattleAction> News()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Sfx("Raven", 0f);
			yield return PerformAction.Animation(this, "shoot3", 0.5f, null, 0f, -1);
			yield return new AddCardsToDiscardAction(Library.CreateCards<AyaNews>(base.Count2, false), AddCardsType.Normal);
			yield break;
		}

		// Token: 0x060007E7 RID: 2023 RVA: 0x00011960 File Offset: 0x0000FB60
		protected override void UpdateMoveCounters()
		{
			int num;
			if (this.HasGraze)
			{
				num = this.GrazeCountDown - 1;
				this.GrazeCountDown = num;
				if (this.GrazeCountDown <= 0)
				{
					this.Next = Raven.MoveType.Graze;
					return;
				}
			}
			num = base.CountDown - 1;
			base.CountDown = num;
			this.Next = ((base.CountDown <= 0) ? Raven.MoveType.AddCard : Raven.MoveType.Shoot);
		}

		// Token: 0x020006FB RID: 1787
		private enum MoveType
		{
			// Token: 0x04000978 RID: 2424
			Shoot,
			// Token: 0x04000979 RID: 2425
			AddCard,
			// Token: 0x0400097A RID: 2426
			Graze
		}
	}
}
