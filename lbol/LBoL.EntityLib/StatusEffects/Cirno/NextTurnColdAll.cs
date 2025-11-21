using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Cirno
{
	// Token: 0x020000E7 RID: 231
	[UsedImplicitly]
	public sealed class NextTurnColdAll : StatusEffect
	{
		// Token: 0x0600033C RID: 828 RVA: 0x000089AC File Offset: 0x00006BAC
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600033D RID: 829 RVA: 0x000089CB File Offset: 0x00006BCB
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			int num;
			for (int i = 0; i < base.Level; i = num + 1)
			{
				if (base.Battle.BattleShouldEnd)
				{
					yield break;
				}
				foreach (EnemyUnit enemyUnit in base.Battle.AllAliveEnemies)
				{
					yield return new ApplyStatusEffectAction<Cold>(enemyUnit, default(int?), default(int?), default(int?), default(int?), 0f, true);
				}
				IEnumerator<EnemyUnit> enumerator = null;
				num = i;
			}
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
			yield break;
		}
	}
}
