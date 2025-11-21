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
	// Token: 0x020000F7 RID: 247
	[UsedImplicitly]
	public sealed class TurnStartPurify : StatusEffect
	{
		// Token: 0x06000376 RID: 886 RVA: 0x00008F40 File Offset: 0x00007140
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted), (GameEventPriority)99999);
			base.HandleOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new GameEventHandler<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000377 RID: 887 RVA: 0x00008F96 File Offset: 0x00007196
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			base.NotifyActivating();
			if (base.Battle.BattleMana.HasTrivial)
			{
				yield return ConvertManaAction.Purify(base.Battle.BattleMana, base.Level);
			}
			yield break;
		}

		// Token: 0x06000378 RID: 888 RVA: 0x00008FA6 File Offset: 0x000071A6
		private void OnPlayerTurnEnding(UnitEventArgs args)
		{
			this.React(new RemoveStatusEffectAction(this, true, 0.1f));
		}
	}
}
