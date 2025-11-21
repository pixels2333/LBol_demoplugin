using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.EntityLib.Cards.Neutral.NoColor;

namespace LBoL.EntityLib.Exhibits.Shining
{
	// Token: 0x0200013F RID: 319
	[UsedImplicitly]
	public sealed class TianwenWangyuanjing : ShiningExhibit
	{
		// Token: 0x06000460 RID: 1120 RVA: 0x0000BA61 File Offset: 0x00009C61
		protected override void OnEnterBattle()
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnStarted, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnStarted));
		}

		// Token: 0x06000461 RID: 1121 RVA: 0x0000BA85 File Offset: 0x00009C85
		private IEnumerable<BattleAction> OnPlayerTurnStarted(GameEventArgs args)
		{
			if (base.Battle.Player.TurnCounter == 1)
			{
				base.NotifyActivating();
				yield return new AddCardsToHandAction(Library.CreateCards<Astrology>(base.Value1, false), AddCardsType.Normal);
			}
			yield break;
		}
	}
}
