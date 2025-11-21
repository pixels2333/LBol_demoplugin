using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Sakuya
{
	// Token: 0x0200001B RID: 27
	[UsedImplicitly]
	public sealed class MihuanJackSe : StatusEffect
	{
		// Token: 0x06000039 RID: 57 RVA: 0x00002630 File Offset: 0x00000830
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x0600003A RID: 58 RVA: 0x0000264F File Offset: 0x0000084F
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<TimeAuraSe>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
