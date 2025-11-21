using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy;

namespace LBoL.EntityLib.EnemyUnits.Normal
{
	// Token: 0x020001D8 RID: 472
	[UsedImplicitly]
	public sealed class HardworkRabbit : EnemyUnit
	{
		// Token: 0x170000B0 RID: 176
		// (get) Token: 0x06000737 RID: 1847 RVA: 0x000105F7 File Offset: 0x0000E7F7
		// (set) Token: 0x06000738 RID: 1848 RVA: 0x000105FF File Offset: 0x0000E7FF
		private HardworkRabbit.MoveType Next { get; set; }

		// Token: 0x170000B1 RID: 177
		// (get) Token: 0x06000739 RID: 1849 RVA: 0x00010608 File Offset: 0x0000E808
		private bool Accuracy
		{
			get
			{
				return base.HasStatusEffect<AccuracyModule>();
			}
		}

		// Token: 0x170000B2 RID: 178
		// (get) Token: 0x0600073A RID: 1850 RVA: 0x00010610 File Offset: 0x0000E810
		// (set) Token: 0x0600073B RID: 1851 RVA: 0x00010618 File Offset: 0x0000E818
		private bool MoonBag { get; set; }

		// Token: 0x0600073C RID: 1852 RVA: 0x00010624 File Offset: 0x0000E824
		protected override void OnEnterBattle(BattleController battle)
		{
			this.Next = HardworkRabbit.MoveType.Attack;
			this.BaseDamage = base.Damage1;
			this.MoonBag = true;
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new Func<GameEventArgs, IEnumerable<BattleAction>>(this.OnBattleStarted));
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new Func<UnitEventArgs, IEnumerable<BattleAction>>(this.OnPlayerTurnStarted));
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x0001068A File Offset: 0x0000E88A
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs arg)
		{
			yield return new ApplyStatusEffectAction<InvincibleEternal>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			yield return PerformAction.Chat(this, "Chat.HardworkRabbit1".Localize(true), 3f, 0.5f, 0f, true);
			yield break;
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x0001069A File Offset: 0x0000E89A
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs arg)
		{
			if (this.MoonBag)
			{
				if (base.Battle.HandIsFull)
				{
					yield return new AddCardsToDrawZoneAction(Library.CreateCards<MoonTipsBag>(1, false), DrawZoneTarget.Top, AddCardsType.Normal);
				}
				else
				{
					yield return new AddCardsToHandAction(new Card[] { Library.CreateCard<MoonTipsBag>() });
				}
				this.MoonBag = false;
			}
			yield break;
		}

		// Token: 0x170000B3 RID: 179
		// (get) Token: 0x0600073F RID: 1855 RVA: 0x000106AA File Offset: 0x0000E8AA
		// (set) Token: 0x06000740 RID: 1856 RVA: 0x000106B2 File Offset: 0x0000E8B2
		private int BaseDamage { get; set; }

		// Token: 0x06000741 RID: 1857 RVA: 0x000106BB File Offset: 0x0000E8BB
		protected override IEnumerable<IEnemyMove> GetTurnMoves()
		{
			HardworkRabbit.MoveType next = this.Next;
			if (next != HardworkRabbit.MoveType.Attack)
			{
				if (next != HardworkRabbit.MoveType.Buff)
				{
					throw new ArgumentOutOfRangeException();
				}
				yield return new SimpleEnemyMove(Intention.PositiveEffect().WithMoveName(base.GetMove(1)), this.BuffActions());
			}
			else
			{
				yield return new SimpleEnemyMove(Intention.Attack(this.BaseDamage, 2, this.Accuracy).WithMoveName(base.GetMove(0)), this.MoonRabbitMove());
				yield return new SimpleEnemyMove(Intention.Attack(this.BaseDamage * 2, true), this.MoonRabbitAttack());
			}
			yield break;
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x000106CB File Offset: 0x0000E8CB
		private IEnumerable<BattleAction> MoonRabbitMove()
		{
			yield return new EnemyMoveAction(this, base.GetMove(0), true);
			yield break;
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x000106DB File Offset: 0x0000E8DB
		private IEnumerable<BattleAction> MoonRabbitAttack()
		{
			List<DamageAction> damageActions = new List<DamageAction>();
			DamageAction damageAction = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)this.BaseDamage, this.Accuracy), base.Gun1, GunType.First);
			damageActions.Add(damageAction);
			yield return damageAction;
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			DamageAction damageAction2 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)this.BaseDamage, this.Accuracy), "Instant", GunType.Middle);
			damageActions.Add(damageAction2);
			yield return damageAction2;
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			DamageAction damageAction3 = new DamageAction(this, base.Battle.Player, DamageInfo.Attack((float)(this.BaseDamage * 2), true), "Instant", GunType.Last);
			damageActions.Add(damageAction3);
			yield return damageAction3;
			yield return new StatisticalTotalDamageAction(damageActions);
			this.BaseDamage++;
			yield break;
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x000106EB File Offset: 0x0000E8EB
		private IEnumerable<BattleAction> BuffActions()
		{
			yield return new EnemyMoveAction(this, base.GetMove(1), true);
			yield return PerformAction.Animation(this, "shoot3", 0f, null, 0f, -1);
			if (!base.HasStatusEffect<AccuracyModule>() && base.Battle.Player.HasStatusEffect<Graze>())
			{
				yield return new ApplyStatusEffectAction<AccuracyModule>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			else if (!base.HasStatusEffect<LightingModule>() && base.GameRun.BaseDeck.Count <= 20)
			{
				yield return new ApplyStatusEffectAction<LightingModule>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else if (!base.HasStatusEffect<PurifyModule>())
			{
				yield return new ApplyStatusEffectAction<PurifyModule>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else if (!base.HasStatusEffect<LightingModule>())
			{
				yield return new ApplyStatusEffectAction<LightingModule>(this, new int?(1), default(int?), default(int?), default(int?), 0f, true);
			}
			else if (!base.HasStatusEffect<AccuracyModule>())
			{
				yield return new ApplyStatusEffectAction<AccuracyModule>(this, default(int?), default(int?), default(int?), default(int?), 0f, true);
			}
			else
			{
				yield return new ApplyStatusEffectAction<Firepower>(this, new int?(base.Power), default(int?), default(int?), default(int?), 0f, true);
			}
			yield return PerformAction.Chat(this, "Chat.HardworkRabbit2".Localize(true), 3f, 0f, 1f, true);
			yield break;
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x000106FC File Offset: 0x0000E8FC
		protected override void UpdateMoveCounters()
		{
			HardworkRabbit.MoveType moveType;
			switch (base.TurnCounter % 3)
			{
			case 0:
				moveType = HardworkRabbit.MoveType.Attack;
				break;
			case 1:
				moveType = HardworkRabbit.MoveType.Buff;
				break;
			case 2:
				moveType = HardworkRabbit.MoveType.Attack;
				break;
			default:
				moveType = this.Next;
				break;
			}
			this.Next = moveType;
		}

		// Token: 0x020006B2 RID: 1714
		private enum MoveType
		{
			// Token: 0x0400084B RID: 2123
			Attack,
			// Token: 0x0400084C RID: 2124
			Buff
		}
	}
}
