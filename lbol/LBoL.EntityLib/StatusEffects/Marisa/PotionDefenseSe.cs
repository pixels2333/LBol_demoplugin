using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Marisa
{
	// Token: 0x0200006C RID: 108
	public sealed class PotionDefenseSe : StatusEffect
	{
		// Token: 0x06000178 RID: 376 RVA: 0x00004F2A File Offset: 0x0000312A
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Owner.TurnEnded, new EventSequencedReactor<UnitEventArgs>(this.OnOwnerTurnEnded));
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00004F49 File Offset: 0x00003149
		private IEnumerable<BattleAction> OnOwnerTurnEnded(UnitEventArgs args)
		{
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
