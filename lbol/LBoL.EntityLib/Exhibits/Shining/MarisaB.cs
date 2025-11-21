using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x02000132 RID: 306
	[UsedImplicitly]
	public sealed class MarisaB : ShiningExhibit
	{
		// Token: 0x06000432 RID: 1074 RVA: 0x0000B52A File Offset: 0x0000972A
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000433 RID: 1075 RVA: 0x0000B54E File Offset: 0x0000974E
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<BManaCard>(base.Value1, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
