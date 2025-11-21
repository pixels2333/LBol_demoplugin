using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;

namespace LBoL.EntityLib.Exhibits.Common
{
	// Token: 0x0200016D RID: 365
	[UsedImplicitly]
	public sealed class Heilianhua : Exhibit
	{
		// Token: 0x0600050C RID: 1292 RVA: 0x0000CB24 File Offset: 0x0000AD24
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<GameEventArgs>(base.Battle.BattleStarted, new EventSequencedReactor<GameEventArgs>(this.OnBattleStarted));
			base.HandleBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnded, delegate(UnitEventArgs _)
			{
				base.Blackout = true;
			});
		}

		// Token: 0x0600050D RID: 1293 RVA: 0x0000CB70 File Offset: 0x0000AD70
		private IEnumerable<BattleAction> OnBattleStarted(GameEventArgs args)
		{
			base.NotifyActivating();
			yield return new GainManaAction(base.Mana);
			yield break;
		}

		// Token: 0x0600050E RID: 1294 RVA: 0x0000CB80 File Offset: 0x0000AD80
		protected override void OnLeaveBattle()
		{
			base.Blackout = false;
		}
	}
}
