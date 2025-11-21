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
	// Token: 0x020000EA RID: 234
	[UsedImplicitly]
	public sealed class WhiteWorldSe : StatusEffect
	{
		// Token: 0x06000343 RID: 835 RVA: 0x00008A2E File Offset: 0x00006C2E
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000344 RID: 836 RVA: 0x00008A4D File Offset: 0x00006C4D
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
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
			yield break;
			yield break;
		}
	}
}
