using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;

namespace LBoL.EntityLib.StatusEffects.Basic
{
	// Token: 0x020000F1 RID: 241
	[UsedImplicitly]
	public sealed class NextTurnGainBlock : StatusEffect
	{
		// Token: 0x06000360 RID: 864 RVA: 0x00008D82 File Offset: 0x00006F82
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000361 RID: 865 RVA: 0x00008DA6 File Offset: 0x00006FA6
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new CastBlockShieldAction(base.Owner, base.Owner, base.Level, 0, BlockShieldType.Direct, false);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
