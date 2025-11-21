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
	// Token: 0x02000033 RID: 51
	[UsedImplicitly]
	public sealed class ReimuShrineSe : StatusEffect
	{
		// Token: 0x06000095 RID: 149 RVA: 0x0000308A File Offset: 0x0000128A
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x06000096 RID: 150 RVA: 0x000030AE File Offset: 0x000012AE
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			yield return new ApplyStatusEffectAction<TempSpirit>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield break;
		}
	}
}
