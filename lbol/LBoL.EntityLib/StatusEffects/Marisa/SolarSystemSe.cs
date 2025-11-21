using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Marisa
{
	// Token: 0x0200006F RID: 111
	public sealed class SolarSystemSe : StatusEffect
	{
		// Token: 0x06000182 RID: 386 RVA: 0x00004FF2 File Offset: 0x000031F2
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarting, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarting));
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00005016 File Offset: 0x00003216
		private IEnumerable<BattleAction> OnPlayerTurnStarting(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Charging>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
