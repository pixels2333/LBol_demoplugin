using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x0200001E RID: 30
	[UsedImplicitly]
	public sealed class SakuyaInvestigateSe : StatusEffect
	{
		// Token: 0x06000043 RID: 67 RVA: 0x000026DD File Offset: 0x000008DD
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarting));
		}

		// Token: 0x06000044 RID: 68 RVA: 0x000026FC File Offset: 0x000008FC
		private IEnumerable<BattleAction> OnOwnerTurnStarting(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Owner.IsExtraTurn)
			{
				base.NotifyActivating();
				foreach (BattleAction battleAction in base.DebuffAction<LockedOn>(base.Battle.AllAliveEnemies, base.Level, 0, 0, 0, true, 0.03f))
				{
					yield return battleAction;
				}
				IEnumerator<BattleAction> enumerator = null;
			}
			yield break;
			yield break;
		}
	}
}
