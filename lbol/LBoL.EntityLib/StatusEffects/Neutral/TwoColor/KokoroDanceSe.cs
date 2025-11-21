using System;
using System.Collections.Generic;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.TwoColor
{
	// Token: 0x02000042 RID: 66
	public sealed class KokoroDanceSe : StatusEffect
	{
		// Token: 0x060000CC RID: 204 RVA: 0x0000370C File Offset: 0x0000190C
		protected override void OnAdded(Unit unit)
		{
			foreach (EnemyUnit enemyUnit in base.Battle.AllAliveEnemies)
			{
				base.ReactOwnerEvent<StatusEffectApplyEventArgs>(enemyUnit.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnEnemyStatusEffectAdded));
			}
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.EnemySpawned, new GameEventHandler<UnitEventArgs>(this.OnEnemySpawned));
		}

		// Token: 0x060000CD RID: 205 RVA: 0x0000378C File Offset: 0x0000198C
		private IEnumerable<BattleAction> OnEnemyStatusEffectAdded(StatusEffectApplyEventArgs args)
		{
			if (args.Effect.Type == StatusEffectType.Negative)
			{
				yield return new DamageAction(base.Battle.Player, args.Unit, DamageInfo.Reaction((float)base.Level, false), "假面", GunType.Single);
			}
			yield break;
		}

		// Token: 0x060000CE RID: 206 RVA: 0x000037A3 File Offset: 0x000019A3
		private void OnEnemySpawned(UnitEventArgs args)
		{
			base.ReactOwnerEvent<StatusEffectApplyEventArgs>(args.Unit.StatusEffectAdded, new EventSequencedReactor<StatusEffectApplyEventArgs>(this.OnEnemyStatusEffectAdded));
		}
	}
}
