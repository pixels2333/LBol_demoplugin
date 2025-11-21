using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Neutral.White
{
	// Token: 0x0200003C RID: 60
	public sealed class LoseSpiritSe : StatusEffect
	{
		// Token: 0x060000B7 RID: 183 RVA: 0x00003439 File Offset: 0x00001639
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnStarted));
		}

		// Token: 0x060000B8 RID: 184 RVA: 0x00003458 File Offset: 0x00001658
		private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<SpiritNegative>(base.Battle.Player, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
