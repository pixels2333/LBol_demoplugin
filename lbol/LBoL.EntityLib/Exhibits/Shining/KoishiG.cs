using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200012F RID: 303
	[UsedImplicitly]
	public sealed class KoishiG : ShiningExhibit
	{
		// Token: 0x06000429 RID: 1065 RVA: 0x0000B404 File Offset: 0x00009604
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x0600042A RID: 1066 RVA: 0x0000B428 File Offset: 0x00009628
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter <= base.Value1)
			{
				base.NotifyActivating();
				yield return new GainManaAction(base.Mana);
			}
			yield break;
		}
	}
}
