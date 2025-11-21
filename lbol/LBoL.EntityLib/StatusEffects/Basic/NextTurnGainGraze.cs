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
	// Token: 0x020000F2 RID: 242
	[UsedImplicitly]
	public sealed class NextTurnGainGraze : StatusEffect
	{
		// Token: 0x06000363 RID: 867 RVA: 0x00008DBE File Offset: 0x00006FBE
		protected override void OnAdded(Unit unit)
		{
			base.ReactOwnerEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000364 RID: 868 RVA: 0x00008DE2 File Offset: 0x00006FE2
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			base.NotifyActivating();
			yield return new ApplyStatusEffectAction<Graze>(base.Owner, new int?(base.Level), default(int?), default(int?), default(int?), 0f, true);
			yield return new RemoveStatusEffectAction(this, true, 0.1f);
			yield break;
		}
	}
}
