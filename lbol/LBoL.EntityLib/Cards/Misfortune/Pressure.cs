using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;

namespace LBoL.EntityLib.Cards.Misfortune
{
	// Token: 0x0200034D RID: 845
	[UsedImplicitly]
	public sealed class Pressure : Card
	{
		// Token: 0x06000C4A RID: 3146 RVA: 0x000180F7 File Offset: 0x000162F7
		protected override void OnEnterBattle(BattleController battle)
		{
			base.ReactBattleEvent<UnitEventArgs>(base.Battle.Player.TurnEnding, new EventSequencedReactor<UnitEventArgs>(this.OnPlayerTurnEnding));
		}

		// Token: 0x06000C4B RID: 3147 RVA: 0x0001811B File Offset: 0x0001631B
		private IEnumerable<BattleAction> OnPlayerTurnEnding(UnitEventArgs args)
		{
			if (base.Battle.BattleShouldEnd)
			{
				yield break;
			}
			if (base.Zone == CardZone.Hand)
			{
				base.NotifyActivating();
				if (base.Battle.Player.Power > 0)
				{
					yield return new LosePowerAction(base.Value1);
				}
			}
			yield break;
		}
	}
}
