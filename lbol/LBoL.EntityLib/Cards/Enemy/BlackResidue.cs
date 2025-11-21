using System;
using System.Collections.Generic;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Enemy
{
	// Token: 0x0200035C RID: 860
	public sealed class BlackResidue : Card
	{
		// Token: 0x06000C67 RID: 3175 RVA: 0x000182CB File Offset: 0x000164CB
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C68 RID: 3176 RVA: 0x000182EA File Offset: 0x000164EA
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand)
			{
				base.NotifyActivating();
				yield return base.LoseLifeAction(base.Value1);
			}
			yield break;
		}
	}
}
