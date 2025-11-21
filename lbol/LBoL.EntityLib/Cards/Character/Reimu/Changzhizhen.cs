using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;

namespace LBoL.EntityLib.Cards.Character.Reimu
{
	// Token: 0x020003C8 RID: 968
	[UsedImplicitly]
	public sealed class Changzhizhen : Card
	{
		// Token: 0x06000DA4 RID: 3492 RVA: 0x000198B8 File Offset: 0x00017AB8
		protected override void OnEnterBattle(BattleController battle)
		{
			base.HandleBattleEvent<StatusEffectEventArgs>(base.Battle.Player.StatusEffectRemoved, new GameEventHandler<StatusEffectEventArgs>(this.StatusEffectRemoved), (GameEventPriority)0);
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000DA5 RID: 3493 RVA: 0x0001990A File Offset: 0x00017B0A
		private void StatusEffectRemoved(StatusEffectEventArgs args)
		{
			if (base.Zone == CardZone.Hand && args.Effect.Type == StatusEffectType.Positive)
			{
				base.DeltaValue1 += base.Value2;
			}
		}

		// Token: 0x06000DA6 RID: 3494 RVA: 0x00019935 File Offset: 0x00017B35
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd || base.Zone != CardZone.Hand)
			{
				yield break;
			}
			EnemyUnit randomEnemy = base.Battle.EnemyGroup.Alives.SampleOrDefault(base.Battle.GameRun.BattleRng);
			if (randomEnemy != null)
			{
				base.NotifyActivating();
				yield return PerformAction.Effect(randomEnemy, "Changzhi", 0f, "ReimuBoundaryHit", 0f, PerformAction.EffectBehavior.PlayOneShot, 0.3f);
				yield return new DamageAction(base.Battle.Player, randomEnemy, DamageInfo.Reaction((float)base.Value1, false), "Instant", GunType.Single);
			}
			yield break;
		}
	}
}
