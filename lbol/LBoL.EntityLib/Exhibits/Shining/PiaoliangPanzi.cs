using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000134 RID: 308
	[UsedImplicitly]
	public sealed class PiaoliangPanzi : ShiningExhibit
	{
		// Token: 0x06000438 RID: 1080 RVA: 0x0000B5A4 File Offset: 0x000097A4
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000439 RID: 1081 RVA: 0x0000B5C8 File Offset: 0x000097C8
		private IEnumerable<BattleAction> OnPlayerTurnStarted(UnitEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				Card[] array = Enumerable.ToList<Card>(Enumerable.Where<Card>(base.Battle.HandZone, (Card c) => c.CanUpgrade)).SampleManyOrAll(base.Value1, base.GameRun.BattleRng);
				yield return new UpgradeCardsAction(array);
			}
			yield break;
		}
	}
}
