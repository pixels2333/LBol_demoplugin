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
	[UsedImplicitly]
	public sealed class Cold : StatusEffect
	{
		[UsedImplicitly]
		public int StackDamage
		{
			get
			{
				return this.BaseDamage * this.StackMultiply;
			}
		}
		[UsedImplicitly]
		public int StackMultiply
		{
			get
			{
				return base.GetSeLevel<ColdUp>() + 2;
			}
		}
		[UsedImplicitly]
		public int BaseDamage
		{
			get
			{
				return 9;
			}
		}
		public static bool CanPlayEffect { get; set; }
		public int Times { get; private set; }
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
		public override string UnitEffectName
		{
			get
			{
				return "ColdLoop";
			}
		}
	}
}
