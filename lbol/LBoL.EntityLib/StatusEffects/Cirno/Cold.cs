using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000D6 RID: 214
	[UsedImplicitly]
	public sealed class Cold : StatusEffect
	{
		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060002F9 RID: 761 RVA: 0x0000818F File Offset: 0x0000638F
		[UsedImplicitly]
		public int StackDamage
		{
			get
			{
				return this.BaseDamage * this.StackMultiply;
			}
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060002FA RID: 762 RVA: 0x0000819E File Offset: 0x0000639E
		[UsedImplicitly]
		public int StackMultiply
		{
			get
			{
				return base.GetSeLevel<ColdUp>() + 2;
			}
		}

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060002FB RID: 763 RVA: 0x000081A8 File Offset: 0x000063A8
		[UsedImplicitly]
		public int BaseDamage
		{
			get
			{
				return 9;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060002FC RID: 764 RVA: 0x000081AC File Offset: 0x000063AC
		// (set) Token: 0x060002FD RID: 765 RVA: 0x000081B3 File Offset: 0x000063B3
		public static bool CanPlayEffect { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060002FE RID: 766 RVA: 0x000081BB File Offset: 0x000063BB
		// (set) Token: 0x060002FF RID: 767 RVA: 0x000081C3 File Offset: 0x000063C3
		public int Times { get; private set; }

		// Token: 0x06000300 RID: 768 RVA: 0x000081CC File Offset: 0x000063CC
		protected override void OnAdded(Unit unit)
		{
			base.HandleOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnStarting, delegate(GameEventArgs _)
			{
				Cold.CanPlayEffect = true;
			});
			base.ReactOwnerEvent<GameEventArgs>(base.Battle.AllEnemyTurnStarted, new EventSequencedReactor<GameEventArgs>(this.OnAllEnemyTurnStarted));
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
			this.React(PerformAction.Animation(unit, "hit", 0.3f, null, 0f, -1));
			this.Times = 1;
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0000826C File Offset: 0x0000646C
		private IEnumerable<BattleAction> OnAllEnemyTurnStarted(GameEventArgs args)
		{
			if (base.Owner == null || base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (Cold.CanPlayEffect)
			{
				Cold.CanPlayEffect = false;
				yield return PerformAction.Effect(base.Battle.Player, "ColdLaunch", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			}
			yield return DamageAction.LoseLife(base.Owner, this.BaseDamage, "Cold1");
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0000827C File Offset: 0x0000647C
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Owner == null || base.Battle.BattleShouldEnd || !base.Owner.IsExtraTurn)
			{
				yield break;
			}
			yield return PerformAction.Effect(base.Battle.Player, "ColdLaunch", 0f, null, 0f, PerformAction.EffectBehavior.PlayOneShot, 0f);
			yield return DamageAction.LoseLife(base.Owner, this.BaseDamage, "Cold1");
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}

		// Token: 0x06000303 RID: 771 RVA: 0x0000828C File Offset: 0x0000648C
		public override bool Stack(StatusEffect other)
		{
			bool flag = base.Stack(other);
			if (flag)
			{
				int num = this.Times + 1;
				this.Times = num;
				if (base.GameRun.IsAutoSeed && base.GameRun.JadeBoxes.Empty<JadeBox>() && this.Times == 9)
				{
					base.GameRun.AchievementHandler.UnlockAchievement(AchievementKey.Cold);
				}
				this.React(DamageAction.LoseLife(base.Owner, this.StackDamage, "Cold2"));
			}
			return flag;
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000304 RID: 772 RVA: 0x0000830F File Offset: 0x0000650F
		public override string UnitEffectName
		{
			get
			{
				return "ColdLoop";
			}
		}
	}
}
