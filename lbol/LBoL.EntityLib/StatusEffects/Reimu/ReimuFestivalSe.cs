using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Reimu
{
	// Token: 0x02000030 RID: 48
	[UsedImplicitly]
	public sealed class ReimuFestivalSe : StatusEffect
	{
		// Token: 0x06000088 RID: 136 RVA: 0x00002E6C File Offset: 0x0000106C
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00002E90 File Offset: 0x00001090
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			yield return new ApplyStatusEffectAction<TempFirepower>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
