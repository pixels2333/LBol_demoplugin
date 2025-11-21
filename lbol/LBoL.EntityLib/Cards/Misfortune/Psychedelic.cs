using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x0200034E RID: 846
	[UsedImplicitly]
	public sealed class Psychedelic : Card
	{
		// Token: 0x06000C4D RID: 3149 RVA: 0x00018133 File Offset: 0x00016333
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnTurnEnding));
		}

		// Token: 0x06000C4E RID: 3150 RVA: 0x00018157 File Offset: 0x00016357
		private IEnumerable<BattleAction> OnTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand)
			{
				base.NotifyActivating();
				yield return base.LoseLifeAction(base.Value1);
				yield return new GainPowerAction(base.Value2);
			}
			yield break;
		}
	}
}
