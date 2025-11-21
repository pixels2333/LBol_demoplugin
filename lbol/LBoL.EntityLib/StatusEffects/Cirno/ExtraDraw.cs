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
	// Token: 0x020000DA RID: 218
	[UsedImplicitly]
	public sealed class ExtraDraw : StatusEffect
	{
		// Token: 0x0600030E RID: 782 RVA: 0x000083A7 File Offset: 0x000065A7
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnTurnStarted));
		}

		// Token: 0x0600030F RID: 783 RVA: 0x000083CB File Offset: 0x000065CB
		private IEnumerable<BattleAction> OnTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			yield return new DrawManyCardAction(base.Level);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
